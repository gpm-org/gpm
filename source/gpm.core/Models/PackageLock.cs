namespace gpm.Core.Models;

public sealed class PackageLock
{
    public int Version { get; set; } = 1;

    public List<PackageMeta> Packages { get; set; } = new();
}
