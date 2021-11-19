using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using gpm.core;
using gpm.core.Models;
using gpm.core.Services;
using Microsoft.Extensions.Options;

namespace gpm.cli
{

    public class AppSettings : IAppSettings
    {
        public AppSettings(
            IOptions<CommonSettings> commonImportArgs
        )
        {
            CommonSettings = commonImportArgs;
        }

        public IOptions<CommonSettings> CommonSettings { get; }

        public /*static*/ void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            var sections = new Dictionary<string, object>()
            {
                {nameof(CommonSettings), CommonSettings.Value}
            };

            var jsonString = JsonSerializer.Serialize(sections, options);
            File.WriteAllText(IAppSettings.GetAppSettingsFile(), jsonString);
        }




    }
}
