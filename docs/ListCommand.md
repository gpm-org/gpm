# gpm list

## Name

`gpm list` - Lists all packages currently installed on your machine.

## Synopsis

```gpm
Usage:
  gpm [options] list

Options:
  -?, -h, --help
```

## Description

The `gpm list` command provides a way for you to list all global, tool-path, or local packages installed on your machine. The command lists the package id, version installed, and location.

## Options

* `-?|-h|--help`
  
    Prints out a description of how to use the command.

## Examples

* **`dotnet tool list`**

  Lists all tools installed user-wide on your machine (current user profile).

The output looks like the following example:

```output
[22:17:46 INF] Available packages:
Id      Url
wolvenkit/wolvenkit             https://github.com/WolvenKit/WolvenKit.git
rfuzzo/wolvenmanager            https://github.com/rfuzzo/WolvenManager.git
```

## See also
