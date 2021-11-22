using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using gpm.core.Models;
using gpm.core.Services;
using Microsoft.Extensions.Options;

namespace gpm.Services
{
    public class AppSettings : IAppSettings
    {
        public AppSettings(IOptions<CommonSettings> commonSettings)
        {
            CommonSettings = commonSettings;
        }

        public IOptions<CommonSettings> CommonSettings { get; }

        public /*static*/ void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            var sections = new Dictionary<string, object> { { nameof(CommonSettings), CommonSettings.Value } };

            var jsonString = JsonSerializer.Serialize(sections, options);
            File.WriteAllText(IAppSettings.GetAppSettingsFile(), jsonString);
        }
    }
}
