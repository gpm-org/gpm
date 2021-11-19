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
    public sealed class Package
    {
        public Package(string id)
        {
            ID = id;
        }



        [JsonPropertyName("id")]
        [ProtoMember(1)]
        public string ID { get; }


        [JsonPropertyName("name")]
        [ProtoMember(2)]
        public string? Name { get; set; }

        [JsonPropertyName("assetIndex")]
        [ProtoMember(3)]
        public int AssetIndex { get; set; }

    }
}
