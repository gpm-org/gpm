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
        public PluginModel(string id, string name, int assetIndex)
        {
            ID = id;
            Name = name;
            AssetIndex = assetIndex;
        }



        [JsonPropertyName("id")]
        public string ID { get; }

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("assetIndex")]
        public int AssetIndex { get; }



        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; }

        [JsonPropertyName("installedversion")]
        public string? InstalledVersion { get; set; }

        [JsonPropertyName("installedversions")]
        public Dictionary<string, string> InstalledVersions { get; set; } = new();
    }
}
