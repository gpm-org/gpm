using ProtoBuf;

namespace gpm.Core.Models;

/// <summary>
/// Model for an installed package
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

    // [ProtoMember(4)]
    // public string? Thumbnail { get; }

    /// <summary>
    /// Each slot has a deployment manifest with deploy info (version, deployed files)
    /// </summary>
    [ProtoMember(6)]
    public Dictionary<int, SlotManifest> Slots { get; set; } = new();

    /// <summary>
    /// Each version has a cache manifest with cache file info
    /// </summary>
    [ProtoMember(7)]
    public Dictionary<string, CacheManifest> CacheData { get; set; } = new();

    public override string ToString() => Key;
}
