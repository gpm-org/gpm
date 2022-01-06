using ProtoBuf;

namespace gpm.Core.Models;

/// <summary>
/// Holds cached file info for versions
/// </summary>
[ProtoContract]
public sealed record CacheManifest
{
    [ProtoMember(1)]
    public HashedFile[]? Files { get; set; }
}
