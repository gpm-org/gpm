# gpm remove

## Name

`gpm remove` - Uninstalls the specified package from your machine.

## Synopsis

```gpm
Usage:
  gpm [options] remove <name>

Arguments:
  <name> 

Options:
  -p, --path <path>  
  -g, --global       
  -s, --slot <slot>  
  -?, -h, --help
```

## Description

The `gpm remove` command provides a way for you to uninstall gpm packages from your machine. To use the command, you specify one of the following options:

* To uninstall a global package that was installed in the default location, use the `--global` option.
* To uninstall a global package that was installed in a custom location,  use the `--path` option.
* To uninstall a local package, omit the `--global` and `--path` options.

## Arguments

* **`PACKAGE_NAME`**

  Name/ID of the package to uninstall. You can find the package name using the [gpm list](ListCommand.md) command.

## Options

* **`-g|--global`**

    Specifies that the package to be removed is from a user-wide installation. Can't be combined with the `--path` option. Omitting both `--global` and `--path` specifies that the package to be removed is a local package.

* **`-p|--path <PATH>`**

    Specifies the location where to uninstall the package. `PATH` can be absolute or relative. Can't be combined with the `--global` option. Omitting both `--global` and `--path` specifies that the package to be removed is a local package.

* **`-s|--slot <SLOT>`**

    The package slot to uninstall. You can find the package slot using the [gpm list](ListCommand.md) command.

* **`-?|-h|--help`**
  
    Prints out a description of how to use the command.

## Examples

* **`gpm remove -g wolvenkit`**

  Uninstalls the [wolvenkit](https://github.com/WolvenKit/Wolvenkit) global tool.

* **`gpm remove wolvenkit --path c:\global-tools`**

  Uninstalls the [wolvenkit](https://github.com/WolvenKit/Wolvenkit) global tool from a specific Windows directory.

* **`gpm remove wolvenkit --path ~/bin`**

  Uninstalls the [wolvenkit](https://github.com/WolvenKit/Wolvenkit) global tool from a specific Linux/macOS directory.

* **`gpm remove wolvenkit`**

  Uninstalls the [wolvenkit](https://github.com/WolvenKit/Wolvenkit) local tool from the current directory.

## See also
