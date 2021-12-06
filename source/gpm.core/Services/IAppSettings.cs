using System;
using System.IO;
using gpm.core.Models;
using Microsoft.Extensions.Options;

namespace gpm.core.Services
{
    public interface IAppSettings
    {
        IOptions<CommonSettings> CommonSettings { get; }

        void Save();

        public static string GetAppDataFolder()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Constants.APPDATA
            );
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        public static string GetLibraryFolder()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "gpm",
                "tools"
            );
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        public static string GetDefaultInstallDir(Package package) => Path.Combine(IAppSettings.GetLibraryFolder(), package.Id);

        public static string GetAppSettingsFile() => Path.Combine(GetAppDataFolder(), Constants.APPSETTINGS);

        public static string GetDbFile() => Path.Combine(GetAppDataFolder(), Constants.DB);

        public static string GetLocalDbFile() => Path.Combine(GetAppDataFolder(), Constants.LOCALDB);


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
}
