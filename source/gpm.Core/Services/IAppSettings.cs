using gpm.Core.Models;
using Microsoft.Extensions.Options;

namespace gpm.Core.Services;

public interface IAppSettings
{
    IOptions<CommonSettings> CommonSettings { get; }

    void Save();

    public static string GetAppDataFolder()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Constants.APPDATA
        );
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        return folder;
    }

    public static string GetSharedAppDataFolder()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Constants.GPM
        );
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        return folder;
    }

    public static string GetLibraryFolder() => Path.Combine(GetSharedAppDataFolder(), "tools");

    public static string GetLocalDbFile() => Path.Combine(GetSharedAppDataFolder(), Constants.LOCALDB);

    public static string GetDefaultInstallDir(Package package)
        => Path.Combine(
            IAppSettings.GetLibraryFolder(),
            package.Id
                .Replace(Path.DirectorySeparatorChar, '_')
                .Replace(Path.AltDirectorySeparatorChar, '_')
            );

    public static string GetAppSettingsFile() => Path.Combine(GetAppDataFolder(), Constants.APPSETTINGS);

    public static string GetDbFile() => Path.Combine(GetAppDataFolder(), Constants.DB);


    public static string GetCacheFolder()
    {
        var folder = Path.Combine(GetAppDataFolder(),
            Constants.APPDATA_CACHE
        );
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        return folder;
    }

    public static string GetLogsFolder()
    {
        var folder = Path.Combine(GetAppDataFolder(),
            Constants.APPDATA_LOGS
        );
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        return folder;
    }

    public static string GetGitDbFolder()
    {
        var folder = Path.Combine(GetAppDataFolder(),
            Constants.APPDATA_DB
            );
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        return folder;
    }
}
