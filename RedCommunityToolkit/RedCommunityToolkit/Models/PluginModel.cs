using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedCommunityToolkit.Models
{
    /// <summary>
    /// Model for a plugin
    /// </summary>
    public sealed class PluginModel
    {
        public PluginModel(string id, string name)
        {
            ID = id;
            Name = name;
        }

        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("installedversion")]
        public string? InstalledVersion { get; set; }

        [JsonPropertyName("installedversions")]
        public IEnumerable<string>? InstalledVersions { get; set; }
    }
}
