using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ProtoBuf;

namespace gpm.core.Models
{
    /// <summary>
    /// Model for a plugin
    /// </summary>

    [ProtoContract]
    public sealed class PackageModel
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PackageModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {

        }

        public PackageModel(string id)
        {
            ID = id;
        }


        [JsonPropertyName("id")]
        [ProtoMember(1)]
        public string ID { get; set; }



        [JsonPropertyName("url")]
        [ProtoMember(2)]
        public string? Url { get; set; }

        [JsonPropertyName("thumbnail")]
        [ProtoMember(4)]
        public string? Thumbnail { get; }

        [JsonPropertyName("installedversion")]
        [ProtoMember(5)]
        public string? InstalledVersion { get; set; }


        [JsonPropertyName("installedversions")]
        [ProtoMember(6)]
        public Dictionary<string, VersionInfo> InstalledVersions { get; set; } = new();



    }

    [ProtoContract]
    public record class VersionInfo
    {
        [JsonPropertyName("deployedfiles")]
        [ProtoMember(1)]
        public string[]? DeployedFiles { get; set; }



    }
}
