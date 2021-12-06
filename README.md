# gpm - a GitHub Package Manager

A Github package manager to install, update and manage **releases** published on GitHub.

The manager registry is located at <https://github.com/rfuzzo/gpm-db> and accepts pull requests. For the package format see below.

## Installation

gpm is a command-line tool that is best installed as a `dotnet global tool`. Open a terminal and type in the following to install `gpm`:

```gpm
dotnet tool install --global gpm
gpm --help
```

For more informations see: <https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools>

## Usage

>See also: [Usage](/docs/Usage.md)

Navigate to a folder to install a package in (but you can also specify the install directory with `-s`).

- [Search](/docs/SearchCommand.md) all available packages: `gpm search`
- [Install](/docs/InstallCommand.md) a package in the current directory: `gpm install <NAME>`
- [Update](/docs/UpdateCommand.md) a package in the current directory `gpm update <NAME>`
- [Uninstall](/docs/RemoveCommand.md) a package in the current directory `gpm remove <NAME>`
- [List](/docs/ListCommand.md) all installed packages: `gpm list`
- [Restore](/docs/RestoreCommand.md) all packages listed in a lock file: `gpm restore`

## Package Format

>See also: [The package format](/docs/PackageFormat.md)

The package format is a simple json file (renamed to `.gpak`) and can be auto generated with the **[gpm-util](/docs/gpm-util.md)** tool. To integrate your package into the registry, make a pull request to <https://github.com/rfuzzo/gpm-db>.

Example:

```gpm
gpm-util new https://github.com/WolvenKit/WolvenKit.git
```

The package format supports adding multiple packages from one repository:

```gpm
gpm-util new <GIT URL> -i <UNIQUE IDENTIFIER>
```

The package format supports custom logic for versioning, picking specific release asset files, installing the package.

### Versioning logic

`TBD`

### Release logic

- `"AssetIndex": 1` (always pick the 2nd asset file in a release)
- `"AssetNamePattern": "*.zip"` (only use release asset files with the extension ".zip") etc

### Install logic

`TBD`

## Contributing

Do you want to contribute? Community feedback and contributions are highly appreciated!

**For general rules and guidelines see [Contributing](/docs/Contributing.md).**
