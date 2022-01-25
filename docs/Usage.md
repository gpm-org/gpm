# Usage

## Synopsis

```md
Usage:
  gpm [options] [command]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  search <pattern>  Search packages in the gpm registry.
  install <name>    Install a package.
  update <name>     Update an installed package.
  remove <name>     Uninstall a package.
  list              Lists all installed packages.
  restore           Restore packages defined in the local package lock file.
  upgrade           Update the local package registry.
  run <name>        Runs a globally installed gpm package.
```

## Commands list

### Search

>See also: [SearchCommand](/docs/SearchCommand.md)

The `gpm search` command provides a way for you to search the gpm registry for packages that can be installed. The command searches the tool names and metadata such as titles, descriptions, and tags.

### Install

>See also: [InstallCommand](/docs/InstallCommand.md)

The `gpm install` command provides a way for you to install gpm packages on your machine. To use the command, you specify one of the following installation options:

* To install a global package in the default location, use the `--global` option.
* To install a global package in a custom location,  use the `--path` option.
* To install a local package, omit the `--global` and `--path` options.

### Update

>See also: [UpdateCommand](/docs/UpdateCommand.md)

The `gpm update` command provides a way for you to update packages on your machine to the latest stable version of the package. The command uninstalls and reinstalls a package, effectively updating it. To use the command, you specify one of the following options:

* To update a global package that was installed in the default location, use the `--global` option
* To update a global package that was installed in a custom location, use the `--path` option.
* To update a local package, omit both `--global` and `--path` options.

### Remove

>See also: [RemoveCommand](/docs/RemoveCommand.md)

The `gpm remove` command provides a way for you to uninstall gpm packages from your machine. To use the command, you specify one of the following options:

* To uninstall a global package that was installed in the default location, use the `--global` option.
* To uninstall a global package that was installed in a custom location,  use the `--path` option.
* To uninstall a local package, omit the `--global` and `--path` options.

### List

>See also: [ListCommand](/docs/ListCommand.md)

The `gpm list` command provides a way for you to list all global, custom, or local packages installed on your machine. The command lists the package id, version installed, and location.

### Restore

>See also: [RestoreCommand](/docs/RestoreCommand.md)

The `gpm restore` command finds the `gpm-lock.json` file that is in scope for the current directory and installs the tools that are listed in it.

### Run

>See also: [RunCommand](/docs/RunCommand.md)

The `gpm run` command runs a globally installed package with given arguments.