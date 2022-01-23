using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using gpm.Core.Models;
using gpm.Core.Services;
using gpm.Core.Util;
using Serilog;

namespace gpm.Core.Installer;

/// <summary>
/// An auto-installer service based on gpm
/// </summary>
public class AutoInstallerService : IAutoInstallerService
{
    private readonly IGitHubService _gitHubService;
    private readonly IDataBaseService _dataBaseService;
    private readonly ILibraryService _libraryService;

    private int? _slot;

    public AutoInstallerService(
        IGitHubService gitHubService,
        IDataBaseService dataBaseService,
        ILibraryService libraryService)
    {
        _gitHubService = gitHubService;
        _dataBaseService = dataBaseService;
        _libraryService = libraryService;
    }

    public bool IsEnabled { get; private set; }



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
        if (_channel?.Package is null || _version is null || !IsEnabled)
        {
            return null;
        }

        return (await _gitHubService.TryGetRelease(_channel.Package, _version))
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
        // TODO: WINDOWS?
        if (!await GpmUtil.RunGpmAsync(GpmUtil.ECommand.run, GetInstallerId(_framework), $"/Restart=WpfAppTest.exe /Dir={AppContext.BaseDirectory} /Slot={_slot}"))
        {
            return false;
        }

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
        if (_channel?.Package is null || _version is null || !IsEnabled)
        {
            return false;
        }

        // download
        if (await _gitHubService.DownloadAssetToCache(_channel.Package, release))
        {
            return true;
        }

        Log.Warning("Failed to download package {Package}", _channel.Package);
        return false;

    }



    public enum EFramework
    {
        NONE,
        WPF,
        WINUI,
        AVALONIA
    }

    private EFramework _framework;
    private static string GetInstallerId(EFramework framework) => framework switch
    {
        EFramework.NONE => throw new ArgumentNullException(nameof(framework)),
        EFramework.WPF => "rfuzzo/gpm-installer/wpf",
        EFramework.WINUI or EFramework.AVALONIA => throw new NotImplementedException(),
        _ => throw new ArgumentNullException(nameof(framework)),
    };
    /// <summary>
    /// Use the WPF installer UI
    /// </summary>
    /// <returns></returns>
    public AutoInstallerService UseWPF()
    {
        _framework = EFramework.WPF;

        return this;
    }

    /// <summary>
    /// Use configuration info from a gpm lockfile
    /// </summary>
    /// <returns></returns>
    public AutoInstallerService AddLockFile()
    {
        // read manifest
        if (!TryGetLockFile(out var info))
        {
            Log.Warning("No package lock file found for app");
            return this;
        }

        var id = info.Packages[0].Id;

        // TODO version
        _version = info.Packages[0].Version;

        AddChannel("Default", id);

        return this;
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

    private string? _version;
    /// <summary>
    /// Registers the current app as a specific version.
    /// Use AddLockFile instead
    /// </summary>
    /// <returns></returns>
    public AutoInstallerService AddVersion(string version)
    {
        _version = version;

        return this;
    }

    public record class Channel(string Name, Package Package);
    private readonly Dictionary<string, Channel> _channels = new();
    /// <summary>
    /// Adds an update channel to the auto-installer
    /// </summary>
    /// <param name="channelName">the name to use for the channel</param>
    /// <param name="id">the gpm id for the app</param>
    /// <returns></returns>
    public AutoInstallerService AddChannel(string channelName, string id)
    {
        // remove duplicate channels for manual register
        var remove = _channels.Where(x => x.Value.Package.Id == id).ToList();
        foreach (var item in remove)
        {
            _channels.Remove(item.Key);
        }

        if (!_channels.ContainsKey(channelName))
        {
            // test if package exists
            var package = _dataBaseService.GetPackageFromName(id);
            if (package is null)
            {
                Log.Warning("_package {NameInner} not found in database", id);
                return this;
            }

            _channels.Add(channelName, new Channel(channelName, package));
        }

        return this;
    }

    private Channel? _channel;
    /// <summary>
    /// Use this Channel to receive updates for this app
    /// </summary>
    /// <param name="channelName">the name of the update channel</param>
    /// <returns></returns>
    public AutoInstallerService UseChannel(string channelName)
    {
        if (_channels.ContainsKey(channelName))
        {
            _channel = _channels[channelName];
        }
        else
        {
            Log.Error("{Channel} not registered", channelName);
        }

        return this;
    }

    /// <summary>
    /// Initializes the update manager
    /// </summary>
    /// <returns></returns>
    public void Build()
    {
        // Checks
        if (_channels.Count == 0)
        {
            Log.Error("No update channel registered");
            return;
        }
        if (_channel == null)
        {
            Log.Error("No update channel registered");
            return;
        }
        if (string.IsNullOrEmpty(_version))
        {
            Log.Error("No version registered for {Package} installation failed", _channel.Package);
            return;
        }

        var package = _channel.Package;

        var result = Nito.AsyncEx.AsyncContext.Run(() => InstallHelperAsync());
        if (!result)
        {
            Log.Warning("Helper installation failed.");
            return;
        }

        // TODO: register app in gpm if not already
        // TODO: lockfiles? did I miss anything?
        _slot = _libraryService.RegisterInSlot(package, AppContext.BaseDirectory, _version);

        IsEnabled = true;
        Log.Information("[{_package}, v.{_version}] auto-update Enabled: {IsEnabled}", package, _version, IsEnabled);
    }


    private async Task<bool> InstallHelperAsync()
    {
        if (!await GpmUtil.EnsureGpmInstalled())
        {
            return false;
        }

        if (!await GpmUtil.RunGpmAsync(GpmUtil.ECommand.install, GetInstallerId(_framework), "-g"))
        {
            return false;
        }

        return true;
    }

}
