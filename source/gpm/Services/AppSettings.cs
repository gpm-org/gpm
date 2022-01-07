using System.Text.Json;
using System.Text.Json.Serialization;
using gpm.Core.Models;
using gpm.Core.Services;
using Microsoft.Extensions.Options;

namespace gpm.Services;

public class AppSettings : IAppSettings
{
    public AppSettings(IOptions<CommonSettings> commonSettings)
    {
        CommonSettings = commonSettings;
    }

    public IOptions<CommonSettings> CommonSettings { get; }

    public /*static*/ void Save()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var sections = new Dictionary<string, object> { { nameof(CommonSettings), CommonSettings.Value } };

        var jsonString = JsonSerializer.Serialize(sections, options);
        File.WriteAllText(IAppSettings.GetAppSettingsFile(), jsonString);
    }
}
