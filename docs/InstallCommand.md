# gpm install

`gpm install` - Installs the specified gpm package on your machine.

## Synopsis

```md
Usage:
  gpm [options] install <name>

Arguments:
  <name>  

Options:
  -v, --version <version>
  -p, --path <path>
  -g, --global
  -?, -h, --help 
```

## Description

The `gpm install` command provides a way for you to install gpm packages on your machine. To use the command, you specify one of the following installation options:

* To install a global package in the default location, use the `--global` option.
* To install a global package in a custom location,  use the `--path` option.
* To install a local tool, omit the `--global` and `--tool-path` options.

Global package are installed in the following directories by default when you specify the `-g` or `--global` option:

| OS          | Path                          |
|-------------|-------------------------------|
| Linux/macOS | `$HOME/.gpm/tools`         |
| Windows     | `%USERPROFILE%\.gpm\tools` |

Local package are added to a *gpm-lock.json* file in the current directory.

## Arguments

* **`PACKAGE_NAME`**

  Name/ID of the package to uninstall. You can find the package name using the [gpm list](ListCommand.md) command.

## Options

* `-v|--version <VERSION>`

    The version of the package to install. By default, the latest stable package version is installed. Use this option to install preview or older versions of the package.

* `-p|--path <PATH>`

    Specifies the location where to install the global package. `PATH` can be absolute or relative. If PATH doesn't exist, the command tries to create it. Omitting both `--global` and `--path` specifies a local package installation.

* `-g|--global`

    Specifies that the installation is user wide. Can't be combined with the --path option. Omitting both --global and --tool-path specifies a local package installation.

* `-?|-h|--help`
  
    Prints out a description of how to use the command.

## Examples

* **`dotnet tool install -g wolvenkit`**

  Installs [wolvenkit](https://github.com/WolvenKit/Wolvenkit) as a global package in the default location.

* **`dotnet tool install wolvenkit --path c:\global-tools`**

  Installs [wolvenkit](https://github.com/WolvenKit/Wolvenkit) as a global package in a specific Windows directory.

* **`dotnet tool install wolvenkit --tool-path ~/bin`**

  Installs [wolvenkit](https://github.com/WolvenKit/Wolvenkit) as a global package in a specific Linux/macOS directory.

* **`dotnet tool install -g wolvenkit --version 8.4.1`**

  Installs version 8.4.1 of [wolvenkit](https://github.com/WolvenKit/Wolvenkit) as a global package.

* **`dotnet tool install wolvenkit`**

  Installs [wolvenkit](https://github.com/WolvenKit/Wolvenkit) as a local package for the current directory.

## See also
