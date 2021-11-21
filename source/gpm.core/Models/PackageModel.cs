using System.Collections.Generic;
using DynamicData.Kernel;
using ProtoBuf;

namespace gpm.core.Models
{
    /// <summary>
    /// Model for an installed plugin
    /// </summary>

    [ProtoContract]
    public sealed class PackageModel
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PackageModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {

        }

        public PackageModel(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Unique Key to a package in the form RepoOwner-RepoName-UniqueIdentifier
        /// </summary>
        [ProtoMember(1)]
        public string Key { get; set; }


        [ProtoMember(4)]
        public string? Thumbnail { get; }

        [ProtoMember(5)]
        public string? LastInstalledVersion { get; set; }

        [ProtoMember(6)]
        public Dictionary<string, PackageManifestData> Manifests { get; set; } = new();


        public void AddOrUpdateManifest<T>(string version, T manifest) where T : IPackageManifest
        {
            if (!Manifests.ContainsKey(version))
            {
                Manifests.Add(version, new PackageManifestData());
            }

            switch (manifest)
            {
                case CachePackageManifest cachePackageManifest:
                    Manifests[version].CacheManifest = cachePackageManifest;
                    break;
                case DeployPackageManifest deployPackageManifest:
                    Manifests[version].DeployManifest = deployPackageManifest;
                    break;
            }
        }

        public Optional<T> TryGetManifest<T>(string version) where T : IPackageManifest
        {
            if (!Manifests.ContainsKey(version))
            {
                return Optional<T>.None;
            }

            if (typeof(T) == typeof(CachePackageManifest))
            {
                var manifest = Manifests[version].CacheManifest;
                return manifest is T m ? Optional<T>.ToOptional(m) : Optional<T>.None;
            }

            if (typeof(T) == typeof(DeployPackageManifest))
            {
                var manifest = Manifests[version].DeployManifest;
                return manifest is T m ? Optional<T>.ToOptional(m) : Optional<T>.None;
            }

            return Optional<T>.None;
        }



    }

    [ProtoContract]
    public record class PackageManifestData
    {
        [ProtoMember(1)]
        public DeployPackageManifest? DeployManifest { get; set; }

        [ProtoMember(2)]
        public CachePackageManifest? CacheManifest { get; set; }

    }

    public interface IPackageManifest
    {
        public HashedFile[]? Files { get; set; }
    }

    [ProtoContract]
    public record class DeployPackageManifest : IPackageManifest
    {
        [ProtoMember(1)]
        public HashedFile[]? Files { get; set; }
    }

    [ProtoContract]
    public record class CachePackageManifest : IPackageManifest
    {
        [ProtoMember(1)]
        public HashedFile[]? Files { get; set; }
    }

    // [ProtoContract]
    // public record class HashedFile(
    //     string? Name,
    //     byte[]? Sha512,
    //     long? Size);

    [ProtoContract]
    public record class HashedFile
    {
#pragma warning disable 8618
        public HashedFile()
#pragma warning restore 8618
        {

        }
        public HashedFile(string name, byte[]? sha512, long? size)
        {
            Name = name;
            Sha512 = sha512;
            Size = size;
        }

        [ProtoMember(1)]
        public string Name { get; init; }
        [ProtoMember(2)]
        public byte[]? Sha512 { get; init; }
        [ProtoMember(3)]
        public long? Size { get; init; }
    }


}
