using System.Text.Json.Serialization;
using ProtoBuf;

namespace gpm.Core.Models
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

        /// <summary>
        /// Required github repo url
        /// </summary>
        [ProtoMember(1)]
        public string Url { get; set; }

        /// <summary>
        /// Optional unique identifier for multiple packages for one repo
        /// </summary>
        [ProtoMember(2)]
        public string? Identifier { get; set; }

        [ProtoMember(9)]
        public List<PackageMeta>? Dependencies { get; set; }

        // METADATA

        [ProtoMember(10)]
        public string[]? Topics { get; set; }

        [ProtoMember(11)]
        public string? Description { get; set; }

        [ProtoMember(12)]
        public string? Homepage { get; set; }

        [ProtoMember(13)]
        public string? License { get; set; }

        // if package > package type

        // LOGIC

        // VERSION LOGIC

        // useafter? only use

        // blacklist versions

        // whitelist versions

        // ASSET LOGIC

        // Asset name pattern
        // *.zip, *%reponame%*?
        [ProtoMember(102)]
        public string? AssetNamePattern { get; set; }

        /// <summary>
        /// Asset index to download
        /// </summary>
        [ProtoMember(103)]
        public int? AssetIndex { get; set; }


        // INSTALL-LOGIC

        // Ignore tags
        // tags are the lowest prio: any other logic is provided

        // install dir patterns
        // support custom local env vars
        // e.g. %cp77root% for
        /// <summary>
        /// Content Type
        /// </summary>
        [ProtoMember(207)]
        public string? InstallPath { get; set; }

        /// <summary>
        /// Content Type
        /// </summary>
        [ProtoMember(208)]
        public EContentType? ContentType { get; set; }




        [JsonIgnore]
        public string Id => string.IsNullOrEmpty(Identifier) ? $"{Owner}/{Name}" : $"{Owner}/{Name}/{Identifier.ToLower()}";

        [JsonIgnore]
        public string Owner => GetRepoOwner();

        [JsonIgnore]
        public string Name => GetRepoName();



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

        public override string ToString() => Id;
    }
}
