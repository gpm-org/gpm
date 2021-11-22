using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using gpm.core.Models;
using gpm.core.Util;

namespace gpm.core.Services
{
    public interface IDataBaseService
    {
        Dictionary<string, Package> Packages { get; set; }

        void Load();
        void Save();


        bool Contains(string key);
        bool ContainsName(string name);
        bool ContainsOwner(string name);
        bool ContainsUrl(string url);
        Package? Lookup(string key);
        IEnumerable<Package> LookupByName(string name);
        IEnumerable<Package> LookupByOwner(string name);
        IEnumerable<Package> LookupByUrl(string url);

        Package? GetPackageFromName(string name);
    }
}
