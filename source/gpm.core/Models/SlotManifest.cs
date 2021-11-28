using System.Collections.Generic;
using ProtoBuf;

namespace gpm.core.Models
{
    /// <summary>
    /// Holds deployment info for specific slots
    /// </summary>
    [ProtoContract]
    public sealed record SlotManifest
    {
        /// <summary>
        /// Deployed Files in slot
        /// </summary>
        [ProtoMember(1)]
        public List<HashedFile> Files { get; set; } = new();

        /// <summary>
        /// Full path to slot
        /// </summary>
        [ProtoMember(2)]
        public string? FullPath { get; set; }

        /// <summary>
        /// Current installed version in slot
        /// </summary>
        [ProtoMember(3)]
        public string? Version { get; set; }
    }
}
