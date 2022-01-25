# gpm install

`gpm run` - Runs a globally installed gpm package.

## Synopsis

```md
Usage:
  gpm run <name> [options]

Arguments:
  <name>  The package name. Can be a github repo url, a repo name or in the form of owner/name/id.

Options:
  -a, --args <args>
  -?, -h, --help
```

## Description

The `gpm run` command runs a globally installed package with given arguments.

The package must be globally installed for this command to work.

## Arguments

* **`PACKAGE_NAME`**

  Name/ID of the package to run. You can find the package name using the [gpm list](ListCommand.md) command.

## Options

* **`-a, --args <args>`**

    Optional commandline arguments to run the package app.

* **`-?|-h|--help`**
  
    Prints out a description of how to use the command.

## Examples

* **`run gpm-org/gpm.installer/wpf -a "/Test /Restart"`**

  Runs the globally installed [gpm.Installer](https://github.com/gpm-org/gpm.Installer) with the arguments `/Test` and `/Restart`.

## See also
