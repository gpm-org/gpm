using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using gpm.Core.Exceptions;
using gpm.Core.Models;
using gpm.Core.Services;
using gpm.Core.Util;
using gpm.Core.Util.Builders;
using Serilog;

namespace gpm.Core.Installer;

/// <summary>
/// An auto-installer service based on gpm
/// </summary>
public class AutoInstallerService
{
    private readonly IGitHubService _gitHubService;
    private readonly IDataBaseService _dataBaseService;

    private Package? _package;
    public string? Version { get; private set; }

    public bool IsEnabled { get; private set; }

    public AutoInstallerService(IGitHubService gitHubService, IDataBaseService dataBaseService)
    {
        _gitHubService = gitHubService;
        _dataBaseService = dataBaseService;

        Init();
    }

    public Package Package => _package.NotNull();


    /// <summary>
    /// Initializes the update manager
    /// </summary>
    /// <returns>false if no valid lockfile found in the app directory</returns>
    private bool Init()
    {
        // read manifest
        if (!TryGetLockFile(out var info))
        {
            Log.Warning("No package lock file found for app. Auto-updates are not available");
            return false;
        }

        var id = info.Packages[0].Id;
        var package = _dataBaseService.GetPackageFromName(id);
        if (package is null)
        {
            Log.Warning("_package {NameInner} not found in database", id);
            return false;
        }

        _package = package;
        IsEnabled = true;
        Version = info.Packages[0].Version;

        Log.Information("[{_package}, v.{Version}] auto-update Enabled: {IsEnabled}", _package, Version, IsEnabled);

        // TODO: register app in gpm if not already





        return true;
    }

    /// <summary>
    /// Updates the current app, silent
    /// 1 API call
    /// </summary>
    /// <returns>true if update succeeded</returns>
    public async Task<bool> CheckAndUpdate()
    {
        var release = await CheckForUpdate();
        if (release == null)
        {
            return false;
        }

        return await Update(release);
    }

    /// <summary>
    /// reads the apps directory for a lockfile and checks if an update is available
    /// 1 API call
    /// </summary>
    /// <returns>false if no valid lockfile found in the app directory or no update available</returns>
    public async Task<ReleaseModel?> CheckForUpdate()
    {
        if (_package is null || Version is null || !IsEnabled)
        {
            return null;
        }

        return (await _gitHubService.TryGetRelease(_package, Version))
            .Out(out var release)
            ? release
            : null;
    }

    /// <summary>
    /// Updates the current app to the latest version
    /// </summary>
    /// <returns>true if update succeeded</returns>
    public async Task<bool> Update(ReleaseModel release)
    {
        if (!IsEnabled)
        {
            return false;
        }

        if (!await DownloadUpdate(release))
        {
            return false;
        }

        // TODO: user callback confirm

        // start gpm install command


        // shutdown exe
        // use a callback here?
        Environment.Exit(0);

        return true;
    }

    /// <summary>
    /// Downloads the asset to the cache
    /// </summary>
    /// <returns>true if update succeeded</returns>
    public async Task<bool> DownloadUpdate(ReleaseModel release)
    {
        if (!IsEnabled)
        {
            return false;
        }

        // TODO: move this?
        // get correct release asset
        // TODO support multiple asset files?
        var assets = release.Assets;
        ArgumentNullException.ThrowIfNull(assets);
        var assetBuilder = IPackageBuilder.CreateDefaultBuilder<AssetBuilder>(Package);
        var asset = assetBuilder.Build(release.Assets);
        if (asset is null)
        {
            Log.Warning("No release asset found for version LATEST");
            return false;
        }

        // download
        var version = release.TagName;
        if (await _gitHubService.DownloadAssetToCache(Package, asset, version))
        {
            return true;
        }

        Log.Warning("Failed to download package {Package}", Package);
        return false;

    }

    /// <summary>
    /// Ensure that the gpm global tool is installed
    /// and reinstalls it if not
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> EnsureGpmInstalled()
    {
        var installed = await InstallGpmAsync();
        if (!installed)
        {
            return await UpdateGpmAsync();
        }
        return true;
    }


    private static bool TryGetLockFile([NotNullWhen(true)] out PackageLock? packageLock)
    {
        var baseDirectory = AppContext.BaseDirectory;

        Log.Information($"Using BaseDirectory: {baseDirectory}");

        var lockFilePath = Path.Combine(baseDirectory, Constants.GPMLOCK);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        if (File.Exists(lockFilePath))
        {
            try
            {
                packageLock = JsonSerializer.Deserialize<PackageLock>(File.ReadAllText(lockFilePath), options);
                if (packageLock is null)
                {
                    return false;
                }
                var version = packageLock.Packages[0].Version;
                if (version is null)
                {
                    return false;
                }
                var id = packageLock.Packages[0].Id;
                if (id is null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to read existing lock file");
            }
        }
        packageLock = null;
        return false;
    }

    private static async Task<bool> UpdateGpmAsync() => await DotnetUtil.RunDotnetToolAsync("update", "gpm");

    private static async Task<bool> InstallGpmAsync() => await DotnetUtil.RunDotnetToolAsync("install", "gpm");

    
}
