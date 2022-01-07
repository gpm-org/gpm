using ProtoBuf;

namespace gpm.Core.Models;

[ProtoContract]
public sealed record HashedFile
{
    public HashedFile()
    {

    }
    public HashedFile(string name, byte[]? sha512, long? size)
    {
        Name = name;
        Sha512 = sha512;
        Size = size;
    }

    [ProtoMember(1)]
    public string? Name { get; set; }
    [ProtoMember(2)]
    public byte[]? Sha512 { get; set; }
    [ProtoMember(3)]
    public long? Size { get; set; }
}
