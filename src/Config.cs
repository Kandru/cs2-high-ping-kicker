using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using System.Text.Json.Serialization;

namespace HighPingKicker
{
    public class WhitelistConfig
    {
        [JsonPropertyName("name")] public string Name { get; set; } = "";
    }

    public class PluginConfig : BasePluginConfig
    {
        // whether the plugin is enabled
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        // debug prints
        [JsonPropertyName("debug")] public bool Debug { get; set; } = false;
        // check interval in seconds
        [JsonPropertyName("check_interval")] public float CheckInterval { get; set; } = 2f;
        // amount pings to collect
        [JsonPropertyName("ping_count")] public int PingCount { get; set; } = 5;
        // amount of times to re-check a player before taking action
        [JsonPropertyName("recheck_count")] public int RecheckCount { get; set; } = 5;
        // ping threshold in milliseconds
        [JsonPropertyName("ping_threshold")] public int PingThreshold { get; set; } = 200;
        // whether or not to ignore spectators
        [JsonPropertyName("ignore_spectators")] public bool IgnoreSpectators { get; set; } = true;
        // whether or not to send a message to the players chat
        [JsonPropertyName("send_chat_message")] public bool SendChatMessage { get; set; } = true;
        // whether or not to send an alert to the player
        [JsonPropertyName("send_alert_message")] public bool SendAlertMessage { get; set; } = false;
        // whether or not to inform other players about the action taken
        [JsonPropertyName("inform_others")] public bool InformOthers { get; set; } = true;
        // action to take when a player has a high ping (kick, spectate)
        [JsonPropertyName("action")] public string Action { get; set; } = "kick";
        // kick reason
        [JsonPropertyName("kick_reason")] public int KickReason { get; set; } = 39;
        // whitelist
        [JsonPropertyName("whitelist")] public Dictionary<string, WhitelistConfig> Whitelist { get; set; } = [];
    }

    public partial class HighPingKicker : BasePlugin, IPluginConfig<PluginConfig>
    {
        public required PluginConfig Config { get; set; }

        public void OnConfigParsed(PluginConfig config)
        {
            Config = config;
            // update config and write new values from plugin to config file if changed after update
            Config.Update();
            Console.WriteLine(Localizer["core.config"]);
        }
    }
}
