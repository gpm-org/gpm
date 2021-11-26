using System.Collections.Generic;
using DynamicData.Kernel;
using gpm.core.Models;

namespace gpm.core.Services
{
    public interface ILibraryService : IDictionary<string, PackageModel>
    {
        void Load();
        void Save();

        PackageModel GetOrAdd(Package package);

        bool IsInstalled(string key);
        bool IsInstalled(Package package);

        bool IsInstalledInSlot(Package package, int slot);
    }
}
