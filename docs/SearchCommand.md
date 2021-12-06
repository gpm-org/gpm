# gpm search

`gpm search` - Searches all packages that are published in the gpm registry: [https://github.com/rfuzzo/gpm-db]

## Synopsis

```md
Usage:
  gpm [options] search <pattern>

Arguments:
  <pattern>

Options:
  -?, -h, --help
```

## Description

The `gpm search` command provides a way for you to search the gpm registry for packages that can be installed. The command searches the tool names and metadata such as titles, descriptions, and tags.

## Options

* `-?|-h|--help`
  
    Prints out a description of how to use the command.

## Examples

Search the gpm registry for packages that begin with "wolven-" in their package name:

```bash
gpm search wolven*
```

The output looks like the following example:

```output
[22:17:46 INF] Available packages:
Id      Url
wolvenkit/wolvenkit             https://github.com/WolvenKit/WolvenKit.git
rfuzzo/wolvenmanager            https://github.com/rfuzzo/WolvenManager.git
```

## See also
