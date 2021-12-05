using System.Collections.Generic;

namespace gpm.core.Models
{
    public sealed class PackageLock
    {
        public int Version { get; set; } = 1;

        public List<PackageMeta> Packages { get; set; } = new();
    }
}
