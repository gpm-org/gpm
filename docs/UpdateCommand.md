# gpm update

## Name

`gpm update` - Updates the specified package on your machine.

## Synopsis

```md
Arguments:
  <name>

Options:
  -g, --global
  -p, --path <path>
  -s, --slot <slot>
  -v, --version <version> 
  -?, -h, --help
```

## Description

The `gpm update` command provides a way for you to update packages on your machine to the latest stable version of the package. The command uninstalls and reinstalls a package, effectively updating it. To use the command, you specify one of the following options:

* To update a global package that was installed in the default location, use the `--global` option
* To update a global package that was installed in a custom location, use the `--path` option.
* To update a local package, omit both `--global` and `--path` options.

## Arguments

* **`PACKAGE_ID`**

  Name/ID of the gpm package to update. You can find the package name using the [gpm list](ListCommand.md) command.

## Options

* **`-g|--global`**

    Specifies that the update is for a user-wide package. Can't be combined with the `--path` option. Omitting both `--global` and `--path` specifies that the package to be updated is a local package.

* **`--path <PATH>`**

  Specifies the location where the global package is installed. PATH can be absolute or relative. Can't be combined with the `--global` option. Omitting both `--global` and `--path` specifies that the package to be updated is a local package.

* **`-s|--slot <SLOT>`**

    Update a specific slot. Input the index of the slot, default is 0. The package slot to uninstall. You can find the package slot using the [gpm list](ListCommand.md) command.

* **`--version <VERSION>`**

  The version range of the package to update to. This cannot be used to downgrade versions, you must `uninstall` newer versions first.

## Examples

* **`gpm update -g wolvenkit`**

  Updates the [wolvenkit](https://github.com/WolvenKit/Wolvenkit) global package.

* **`gpm update wolvenkit --path c:\global-tools`**

  Updates the [wolvenkit](https://github.com/WolvenKit/Wolvenkit) global package located in a specific Windows directory.

* **`gpm update wolvenkit --path ~/bin`**

  Updates the [wolvenkit](https://github.com/WolvenKit/Wolvenkit) global package located in a specific Linux/macOS directory.

* **`gpm update wolvenkit`**

  Updates the [wolvenkit](https://github.com/WolvenKit/Wolvenkit) local package installed for the current directory.

* **`gpm update -g wolvenkit --version 8.4.2`**

  Updates the [wolvenkit](https://github.com/WolvenKit/Wolvenkit) global package to version `8.4.2`.

## See also
