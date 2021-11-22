using System.Collections.Generic;
using DynamicData.Kernel;
using gpm.core.Models;

namespace gpm.core.Services
{
    public interface ILibraryService
    {
        void Load();
        void Save();

        bool Contains(string key);
        Optional<PackageModel> Lookup(string key);
        PackageModel GetOrAdd(Package package);
        void Remove(string id);

        IEnumerable<PackageModel> GetPackages();
    }
}
