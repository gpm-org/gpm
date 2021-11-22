using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Kernel;
using gpm.core.Models;
using gpm.core.Util;

namespace gpm.core.Services
{
    public interface ILibraryService
    {
        //Dictionary<string, PackageModel> Packages { get; set; }

        void Load();

        bool Contains(string key);

        Optional<PackageModel> Lookup(string key);
        void Save();
        PackageModel GetOrAdd(Package package);
        void Remove(string id);
        IEnumerable<PackageModel> GetPackages();
    }
}
