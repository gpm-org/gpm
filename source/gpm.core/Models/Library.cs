using System.Collections.Generic;
using ProtoBuf;

namespace gpm.core.Models
{
    [ProtoContract]
    public class Library
    {
        [ProtoMember(1)]
        public Dictionary<string, PackageModel> Plugins { get; set; } = new();
    }

}
