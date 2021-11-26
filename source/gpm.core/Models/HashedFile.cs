using ProtoBuf;

namespace gpm.core.Models
{
    [ProtoContract]
    public record HashedFile
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
