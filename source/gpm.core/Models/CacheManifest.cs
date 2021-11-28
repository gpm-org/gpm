using ProtoBuf;

namespace gpm.core.Models
{
    /// <summary>
    /// Holds cached file info for versions
    /// </summary>
    [ProtoContract]
    public sealed record CacheManifest
    {
        [ProtoMember(1)]
        public HashedFile[]? Files { get; set; }
    }
}
