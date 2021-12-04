# gpm - a GitHub Package Manager

A Github package manager to install, update and manage **releases** published on GitHub. 

## Installation
gpm is a commandline tool that is best installed as a dotnet global tool: https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools

```
dotnet tool install --global gpm
gpm --help
```

## Usage
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

## Contributing
Do you want to contribute? Community feedback and contributions are highly appreciated!

**For general rules and guidelines see [CONTRIBUTING.md](/CONTRIBUTING.md).**
