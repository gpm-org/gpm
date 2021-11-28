using System;
using System.IO;
using System.Linq;
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

        // METADATA

        /// <summary>
        /// GitHub tags
        /// </summary>
        [ProtoMember(10)]
        public string[]? Tags { get; set; }

        // Description
        // Owner
        // License?
        // if package > package type

        // LOGIC

        // DOWNLOAD-LOGIC

        // Asset name pattern
        // *.zip, *%reponame%*?

        // useafter? only use

        // blacklist versions

        // whitelist versions





        /// <summary>
        /// Asset index to download
        /// </summary>
        [ProtoMember(103)]
        public int AssetIndex { get; set; }


        // INSTALL-LOGIC

        // Ignore tags
        // tags are the lowest prio: any other logic is provided

        // install dir patterns
        // support custom local env vars
        // e.g. %cp77root% for

        /// <summary>
        /// Content Type
        /// </summary>
        [ProtoMember(208)]
        public EContentType? ContentType { get; set; }









        public string Id => string.IsNullOrEmpty(Identifier) ? $"{RepoOwner}/{RepoName}" : $"{RepoOwner}/{RepoName}/{Identifier.ToLower()}";

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

        public override string ToString() => this.Id;
    }
}
