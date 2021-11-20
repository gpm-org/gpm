using System;
using System.Collections.Generic;
using System.IO;
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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Package()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {

        }

        public Package(string url)
        {
            Url = url;
        }

        [JsonPropertyName("url")]
        [ProtoMember(1)]
        public string Url { get; set; }

        [JsonPropertyName("identifier")]
        [ProtoMember(2)]
        public string? Identifier { get; set; }

        [JsonPropertyName("assetIndex")]
        [ProtoMember(3)]
        public int AssetIndex { get; set; }

        // LOGIC

        // Install Directories


        // ContentType
        [JsonPropertyName("contentType")]
        [ProtoMember(8)]
        public EContentType? ContentType { get; set; }

        // Tags
        [JsonPropertyName("tags")]
        [ProtoMember(9)]
        public string[]? Tags { get; set; }







        public string Id => string.IsNullOrEmpty(Identifier) ? $"{RepoOwner}-{RepoName}" : $"{RepoOwner}-{RepoName}-{Identifier.ToLower()}";

        public string RepoOwner => GetRepoOwner();

        public string RepoName => GetRepoName();




        private string GetRepoOwner()
        {
            var fi = new FileInfo(Url);
            if (fi.Directory is null)
            {
                throw new ArgumentException(nameof(Url));
            }
            var rOwner = fi.Directory.Name;

            if (string.IsNullOrEmpty(rOwner))
            {
                throw new ArgumentException(nameof(Url));
            }

            return rOwner.ToLower();
        }

        private string GetRepoName()
        {
            var fi = new FileInfo(Url);
            if (fi.Directory is null)
            {
                throw new ArgumentException(nameof(Url));
            }
            var rName = fi.Name.Split('.').First();

            if (string.IsNullOrEmpty(rName))
            {
                throw new ArgumentException(nameof(Url));
            }

            return rName.ToLower();
        }
    }
}
