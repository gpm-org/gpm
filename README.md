# gpm - a GitHub Package Manager

A Github package manager to install, update and manage **releases** published on GitHub. 
The manager registry is located at https://github.com/rfuzzo/gpm-db and accepts pull requests. For the package format see below.

## Installation
gpm is a commandline tool that is best installed as a dotnet global tool: https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools

```
dotnet tool install --global gpm
gpm --help
```

## Usage
Navigate to a folder to install a package in (but you can also specify the install directory with `-s`). 

- List all available packages: `gpm list`
- Install a package in the current directory: `gpm install <NAME>`
- List all installed packages: `gpm installed`
- Update a package `gpm update <NAME>`


```
Usage:
  gpm [options] [command]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  list            Lists all available packages.
  install <name>  Install a package.
  update <name>   Update an installed package.
  remove <name>
  installed       Lists all installed packages.
```

## Package Format

The package format is a simple json file (renamed to gpak) and can be auto generated with the **gpm-util** tool. To integrate your package into the registry, make a pull request to https://github.com/rfuzzo/gpm-db 

The package format supports custom logic for 
- multiple packages from one repository: `gpm-util new <GIT URL> -i <UNIQUE IDENTIFIER>`
- versioning logic: `TBD`
- release asset file logic: `"AssetIndex": 1` (always pick the 2nd asset file in a release), `"AssetNamePattern": "*.zip"` (only use release asset files with the extension ".zip") etc
- install logic: `TBD`

Example: `gpm-util new https://github.com/WolvenKit/WolvenKit.git` will create and fetch metadata directly from GitHub and create the following file:

```
{
  "Url": "https://github.com/WolvenKit/WolvenKit.git",
  "Identifier": "",
  "Topics": [
    "witcher-3",
    "game",
    "modding-tools",
    "hacktoberfest",
    "cyberpunk2077"
  ],
  "Description": "Mod editor/creator for RED Engine games. The point is to have an all in one tool for creating mods for the games made with the engine.",
  "Homepage": "http://redmodding.org/",
  "License": "GNU General Public License v3.0",
  "Id": "wolvenkit/wolvenkit",
  "Owner": "wolvenkit",
  "Name": "wolvenkit"
}

```

## Contributing
Do you want to contribute? Community feedback and contributions are highly appreciated!

**For general rules and guidelines see [CONTRIBUTING.md](/CONTRIBUTING.md).**
