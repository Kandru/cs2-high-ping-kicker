using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.ValveConstants.Protobuf;

namespace HighPingKicker
{
    public partial class HighPingKicker : BasePlugin
    {
        public override string ModuleName => "CS2 HighPingKicker";
        public override string ModuleAuthor => "Kalle <kalle@kandru.de>";

        private Dictionary<CCSPlayerController, long> _players = [];
        private Dictionary<CCSPlayerController, List<long>> _lastPings = [];
        private double _lastCheckTime = 0;

        public override void Load(bool hotReload)
        {
            RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
            RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
            RegisterListener<Listeners.OnTick>(OnTick);
            RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
            if (hotReload)
            {
                foreach (CCSPlayerController player in Utilities.GetPlayers().Where(
                    p => !p.IsBot && !p.IsHLTV))
                {
                    DebugPrint($"Player added: {player.PlayerName} (SteamID: {player.NetworkIDString} / {player.SteamID})");
                    _players.Add(player, 0);
                    _lastPings.Add(player, []);
                }
            }
        }

        public override void Unload(bool hotReload)
        {
            DeregisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
            DeregisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
            RemoveListener<Listeners.OnTick>(OnTick);
            RemoveListener<Listeners.OnMapEnd>(OnMapEnd);
        }

        private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (player == null
                || player.IsBot
                || player.IsHLTV
                || Config.Whitelist.ContainsKey(player.SteamID.ToString())
                || Config.Whitelist.ContainsKey(player.NetworkIDString))
            {
                return HookResult.Continue;
            }
            DebugPrint($"Player connected: {player.PlayerName} (SteamID: {player.NetworkIDString} / {player.SteamID})");
            _players.Add(player, 0);
            _lastPings.Add(player, []);
            return HookResult.Continue;
        }

        private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (player == null
                || player.IsBot
                || player.IsHLTV)
            {
                return HookResult.Continue;
            }
            DebugPrint($"Player disconnected: {player.PlayerName} (SteamID: {player.SteamID} / {player.NetworkIDString})");
            _players.Remove(player);
            _lastPings.Remove(player);
            return HookResult.Continue;
        }

        private void OnTick()
        {
            // only check every x seconds
            if (_lastCheckTime >= Server.CurrentTime)
            {
                return;
            }
            // set next check time
            _lastCheckTime = Server.CurrentTime + Config.CheckInterval;
            // check all players
            Dictionary<CCSPlayerController, long> _playersCopy = new(_players);
            foreach (var kvp in _playersCopy)
            {
                // add ping to list
                _lastPings[kvp.Key].Add(kvp.Key.Ping);
                // check if we have enough pings for further checks
                if (_lastPings[kvp.Key].Count < Config.PingCount)
                {
                    // continue with next player
                    continue;
                }
                else
                {
                    // delete oldest ping
                    _lastPings[kvp.Key].RemoveAt(0);
                }
                // ignore spectators if configured
                if (Config.IgnoreSpectators && kvp.Key.Team == CsTeam.Spectator)
                {
                    continue;
                }
                // calculate average ping
                long averagePing = (long)_lastPings[kvp.Key].Average();
                // check if average ping is higher than allowed
                if (averagePing > Config.PingThreshold)
                {
                    // check if player has been checked enough times
                    if (kvp.Value + 1 >= Config.RecheckCount)
                    {
                        // take action
                        switch (Config.Action.ToLower())
                        {
                            case "kick":
                                DebugPrint($"Kicking player: {kvp.Key.PlayerName} (SteamID: {kvp.Key.SteamID}) for high ping ({averagePing} ms)");
                                // kick next frame to avoid issues with hibernation
                                Server.NextFrame(() => kvp.Key.Disconnect((NetworkDisconnectionReason)Config.KickReason));
                                // inform other players if configured
                                if (Config.InformOthers)
                                {
                                    Server.PrintToChatAll(Localizer["message.kick"]
                                        .Value.Replace("{player}", kvp.Key.PlayerName)
                                        .Replace("{ping}", averagePing.ToString()));
                                }
                                break;
                            case "spectate":
                                DebugPrint($"Moving player to spectator: {kvp.Key.PlayerName} (SteamID: {kvp.Key.SteamID}) for high ping ({averagePing} ms)");
                                // move player to spectator team
                                kvp.Key.ChangeTeam(CsTeam.Spectator);
                                // inform other players if configured
                                if (Config.InformOthers)
                                {
                                    Server.PrintToChatAll(Localizer["message.spectate"]
                                        .Value.Replace("{player}", kvp.Key.PlayerName)
                                        .Replace("{ping}", averagePing.ToString()));
                                }
                                break;
                            default:
                                DebugPrint($"Unknown action: {Config.Action} for {kvp.Key.PlayerName} (SteamID: {kvp.Key.SteamID}) with high ping ({averagePing} ms)");
                                break;
                        }
                        // reset checks
                        _players[kvp.Key] = 0;
                    }
                    else
                    {
                        DebugPrint($"Player {kvp.Key.PlayerName} (SteamID: {kvp.Key.SteamID}) has high ping ({averagePing} ms, check {kvp.Value + 1} of {Config.RecheckCount})");
                        // increment checks
                        _players[kvp.Key]++;
                        // announce to player
                        SendMessageToPlayer(kvp.Key, Localizer["player.warning"]
                            .Value.Replace("{ping}", averagePing.ToString())
                            .Replace("{total}", Config.RecheckCount.ToString())
                            .Replace("{number}", (kvp.Value + 1).ToString()));
                    }
                }
                else
                {
                    _players[kvp.Key] = 0;
                }
            }
        }

        private void OnMapEnd()
        {
            // reset all players and pings
            _players.Clear();
            _lastPings.Clear();
            _lastCheckTime = 0;
            DebugPrint("Map ended, resetting player list and pings.");
        }

        private void SendMessageToPlayer(CCSPlayerController player, string message)
        {
            if (Config.SendAlertMessage)
            {
                player.PrintToCenterAlert(message);
            }
            if (Config.SendChatMessage)
            {
                player.PrintToChat(message);
            }

        }
    }
}
