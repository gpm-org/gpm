using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gpm.core.Models;
using gpm.core.Services;
using LibGit2Sharp;
using ProtoBuf;

namespace gpm.core.Util
{
    public static class LibraryUtil
    {
        public static Library LoadLibrary()
        {
            if (File.Exists(IAppSettings.GetLocalDbFile()))
            {
                try
                {
                    Library library;
                    using (var file = File.OpenRead(IAppSettings.GetLocalDbFile()))
                    {
                        library = Serializer.Deserialize<Library>(file);
                    }

                    return library;
                }
                catch (Exception)
                {
                    throw;
                }

            }

            return new Library();
        }
    }
}
