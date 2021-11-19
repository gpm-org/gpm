using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using gpm.core;
using Microsoft.Extensions.Options;

namespace gpm.cli
{
    public interface IAppSettings
    {
        IOptions<CommonSettings> CommonSettings { get; }

        void Save();
    }

    public class CommonSettings
    {
        public bool IsInitialized { get; set; }
    }

    public class AppSettings : IAppSettings
    {
        private readonly IOptions<CommonSettings> _commonSettings;

        public AppSettings(
            IOptions<CommonSettings> commonImportArgs
        )
        {
            _commonSettings = commonImportArgs;
        }

        public IOptions<CommonSettings> CommonSettings => _commonSettings;

        public /*static*/ void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            var sections = new Dictionary<string, object>()
            {
                {nameof(CommonSettings), _commonSettings.Value}
            };

            var jsonString = JsonSerializer.Serialize(sections, options);
            File.WriteAllText(AppSettings.GetAppSettingsFile(), jsonString);
        }

        public static string GetAppSettingsFile() => Path.Combine(GetAppDataFolder(), Constants.APPSETTINGS);
        public static string GetDbFile() => Path.Combine(GetAppDataFolder(), Constants.DB);

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
