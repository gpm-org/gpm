using System.IO;
using gpm.core.Models;
using gpm.core.Services;
using ProtoBuf;

namespace gpm.core.Util
{
    public static class LibraryUtil
    {
        public static Library LoadLibrary()
        {
            if (!File.Exists(IAppSettings.GetLocalDbFile()))
            {
                return new Library();
            }

            using var file = File.OpenRead(IAppSettings.GetLocalDbFile());
            return Serializer.Deserialize<Library>(file);
        }
    }
}
