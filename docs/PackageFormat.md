# The Package Format

The package format is a simple json file (renamed to `.gpak`) and can be auto generated with the **[gpm-util](/docs/gpm-util.md)** tool. To integrate your package into the registry, make a pull request to <https://github.com/rfuzzo/gpm-db>.

Example:

```gpm
gpm-util new https://github.com/WolvenKit/WolvenKit.git
```

 will create and fetch metadata directly from GitHub and create the following file:

```json
{
  "Url": "https://github.com/WolvenKit/WolvenKit.git",
  "Topics": [
    "witcher-3",
    "game",
    "modding-tools",
    "hacktoberfest",
    "cyberpunk2077"
  ],
  "Description": "Mod editor/creator for RED Engine games. The point is to have an all in one tool for creating mods for the games made with the engine.",
  "Homepage": "http://redmodding.org/",
  "License": "GNU General Public License v3.0",
  "Id": "wolvenkit/wolvenkit",
  "Owner": "wolvenkit",
  "Name": "wolvenkit"
}
```

## Multiple packages from one repository

The package format supports adding multiple packages from one repository. You need to specify the `Identifier` property in the package format. The **[gpm-util](/docs/gpm-util.md)** tool can also add a unique identifier:

```gpm
gpm-util new <GIT URL> -i <UNIQUE IDENTIFIER>
```

Example:

```json
{
  "Url": "https://github.com/WolvenKit/WolvenKit.git",
  "Identifier": "test1"
}
```

## Dependencies

The package format supports adding dependencies for a package. You need to specify the `Dependencies` property in the package format. A dependency `Id` must specify a package. Optionally you may specify a `Version` as well. Omitting the version will default to the latest version.

Example:

```json
{
  "Url": "https://github.com/WolvenKit/WolvenKit.git",
  "Identifier": "test2",
  "Dependencies": [
    {
      "Id": "wolvenkit/wolvenkit/test1",
      "Version": "8.4.2"
    },
    {
      "Id": "jac3km4/redscript"
    }
  ]
}
```

## Logic

The package format supports custom logic for versioning, picking specific release asset files, installing the package.

### Versioning logic

`TBD`

### Release logic

- `"AssetIndex": 1` (always pick the 2nd asset file in a release)
- `"AssetNamePattern": "*.zip"` (only use release asset files with the extension ".zip") etc

### Install logic

`TBD`
