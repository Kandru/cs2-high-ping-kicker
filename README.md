# CounterstrikeSharp - High Ping Kicker

[![UpdateManager Compatible](https://img.shields.io/badge/CS2-UpdateManager-darkgreen)](https://github.com/Kandru/cs2-update-manager/)
[![GitHub release](https://img.shields.io/github/release/Kandru/cs2-high-ping-kicker?include_prereleases=&sort=semver&color=blue)](https://github.com/Kandru/cs2-high-ping-kicker/releases/)
[![License](https://img.shields.io/badge/License-GPLv3-blue)](#license)
[![issues - cs2-map-modifier](https://img.shields.io/github/issues/Kandru/cs2-high-ping-kicker)](https://github.com/Kandru/cs2-high-ping-kicker/issues)
[![](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=C2AVYKGVP9TRG)

This plug-in provides a simple high-ping kick mechanism for your Counter-Strike 2 game server. It simply checks the average ping of each player regularly and either kicks them or moves them to the spectator.

## Installation

1. Download and extract the latest release from the [GitHub releases page](https://github.com/Kandru/cs2-high-ping-kicker/releases/).
2. Move the "HighPingKicker" folder to the `/addons/counterstrikesharp/plugins/` directory.
3. Restart the server.

Updating is even easier: simply overwrite all plugin files and they will be reloaded automatically. To automate updates please use our [CS2 Update Manager](https://github.com/Kandru/cs2-update-manager/).


## Configuration

This plugin automatically creates a readable JSON configuration file. This configuration file can be found in `/addons/counterstrikesharp/configs/plugins/HighPingKicker/HighPingKicker.json`.

```json
{
  "enabled": true,
  "debug": true,
  "check_interval": 2,
  "ping_count": 5,
  "recheck_count": 5,
  "ping_threshold": 200,
  "ignore_spectators": true,
  "send_chat_message": true,
  "send_alert_message": false,
  "inform_others": true,
  "action": "spectate",
  "kick_reason": 39,
  "whitelist": {},
  "ConfigVersion": 1
}
```

### enabled

Whether or not this plug-in is enabled.

### debug

Debug mode for troubleshooting (effectively prints debug messages to the console).

### check_interval (in seconds)

How often the logic of this plug-in should run (every 2 seconds by default).

### ping_count

How many pings to gather before checking the average ping of a player. Keep in mind when changing the *check_interval* there will be a collecting phase of *check_interval* multiplied by *ping_count* first. Oldest ping will be deleted every *check_interval* seconds when *ping_count* has been reached. This can be used to slow down or speed up detection.

### recheck_count

How often a player should be re-evaluated when the ping is above *ping_threshold* before a player will be kicked or moved to spectator.

### ping_threshold

Highest allowed ping of a player. Above this limit players will either be kicked or moved to spectator.

### ignore_spectators

Whether or not spectators should be ignored. Make sure to use the action *kick* if you disable this. Otherwise the script will continuesly move the player to spectators.

### send_chat_message

Whether or not to inform players via chat message on each check if their ping is above average.

### send_alert_message

Whether or not to inform players via alert message on each check if their ping is above average. If enabled you should shorten the message via the languages available in the lang-folder.

### inform_others

Whether or not to inform other players via chat what happened to the high-ping player.

### action

Action to take. Make sure it is either *kick* or *spectate*. If you miss-spell it or use some other word no action will be taken and this plug-in is useless.

### kick_reaseon

The reason to kick players with. Can be one of the following list. Make sure it exists or other problems may occure: https://docs.cssharp.dev/api/CounterStrikeSharp.API.ValveConstants.Protobuf.NetworkDisconnectionReason.html

### whitelist

Players that will be ignored. Only the SteamID is necessary. The name is only for your reference and will not be used.

'''json
{
  "whitelist": {
    "123456789": {
      "name": "kalle"
    }
  },
}
'''

## Commands

Commands admins can use inside the server console.

### hpk reload

Reloads the config file and applies it instantly.

## Compile Yourself

Clone the project:

```bash
git clone https://github.com/Kandru/cs2-high-ping-kicker.git
```

Go to the project directory

```bash
  cd cs2-high-ping-kicker
```

Install dependencies

```bash
  dotnet restore
```

Build debug files (to use on a development game server)

```bash
  dotnet build
```

Build release files (to use on a production game server)

```bash
  dotnet publish
```

## FAQ

TODO

## License

Released under [GPLv3](/LICENSE) by [@Kandru](https://github.com/Kandru).

## Authors

- [@derkalle4](https://www.github.com/derkalle4)
