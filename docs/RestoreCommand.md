# gpm restore

## Name

`gpm restore` -  Restore packages defined in the local package lock file.

## Synopsis

```gpm
Usage:
  gpm [options] restore

Options:
  -?, -h, --help
```

## Description

The `gpm restore` command finds the `gpm-lock.json` file that is in scope for the current directory and installs the tools that are listed in it.

A package lock file may look like this:

```json
{
  "Version": 1,
  "Packages": [
    {
      "Id": "wolvenkit/wolvenkit/test2",
      "Version": "8.4.3"
    },
    {
      "Id": "jac3km4/redscript",
      "Version": "v0.4.0-RC1"
    }
  ]
}
```

## Options

- **`-?|-h|--help`**
  
    Prints out a description of how to use the command.

## Example

- **`gpm restore`**

  Restores local packages for the current directory.

## See also
