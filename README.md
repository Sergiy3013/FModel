FModel - An Unreal Engine Archives Explorer in C#
------------------------------------------

[![CI Status](https://img.shields.io/github/actions/workflow/status/4sval/FModel/qa.yml?label=CI)](https://github.com/4sval/FModel/actions)
[![Latest](https://img.shields.io/github/v/release/4sval/FModel?color=yellow)](https://fmodel.app/download)
[![Donate](https://img.shields.io/badge/sponsor-DB61A2?logo=GitHub-Sponsors&logoColor=white)](https://fmodel.app/donate)
[![Discord](https://discord.com/api/guilds/637265123144237061/widget.png?style=shield)](https://fmodel.app/discord)
***

### Description:
FModel is an archive explorer for [Unreal Engine](https://www.unrealengine.com/en-US/) games that uses [CUE4Parse](https://github.com/FabianFG/CUE4Parse) as its core parsing library, providing robust support for the latest UE4 and UE5 archive formats. It aims to deliver a modern and intuitive user interface, powerful features, and a comprehensive set of tools for previewing and converting game packages, empowering YOU to understand games' inner workings with ease.

FModel is actively maintained and developed by a dedicated community of contributors, and welcomes all new contributions and feedback.

### Installation:
For installation, follow the instructions from [here](https://github.com/4sval/FModel/wiki/Installing-FModel)

### Console Mode:
FModel now supports a console mode for command-line operations without launching the GUI.

**Usage:**
```bash
FModel.exe --console <command> [options]
```

**Commands:**
- `info` - Display game and provider information
- `list [pattern]` - List available assets (optional: filter by pattern)
- `extract <path>` - Extract specified asset by path

**Examples:**
```bash
# Show game information
FModel.exe --console info

# List all assets
FModel.exe --console list

# List assets matching a pattern
FModel.exe --console list "FortniteGame"

# Extract a specific asset
FModel.exe --console extract "FortniteGame/Content/Items/Weapons/Rifle.uasset"

# Show help
FModel.exe --console --help
```

**Note:** Console mode requires the same configuration as the GUI mode (game directory, settings, AES keys, etc.).

### Sponsorship:
<p>
  <a href="https://www.jetbrains.com/">
    <img src="https://cdn.fmodel.app/i/svg/jetbrains.svg" width="256px">
  </a>
  <a href="https://1password.com/">
    <picture>
      <source media="(prefers-color-scheme: dark)" srcset="https://cdn.fmodel.app/i/svg/1password-light.svg">
      <source media="(prefers-color-scheme: light)" srcset="https://cdn.fmodel.app/i/svg/1password-dark.svg">
      <img src="https://cdn.fmodel.app/i/svg/1password-light.svg" width="256px">
    </picture>
  </a>
</p>

### License:
FModel is licensed under [GPL-3](https://github.com/4sval/FModel/blob/dev/LICENSE), and licenses of third-party libraries used are listed [here](https://github.com/4sval/FModel/blob/dev/NOTICE).