using Microsoft.Extensions.Options;

namespace gpm.core.Services
{
    public interface IAppSettings
    {
        IOptions<CommonSettings> CommonSettings { get; }

        void Save();


        public static string GetAppSettingsFile() => Path.Combine(GetAppDataFolder(), Constants.APPSETTINGS);
        public static string GetDbFile() => Path.Combine(GetAppDataFolder(), Constants.DB);
        public static string GetLocalDbFile() => Path.Combine(GetAppDataFolder(), Constants.LOCALDB);

        public static string GetAppDataFolder()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "gpm"
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
                "db"
                );
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }
    }
}
