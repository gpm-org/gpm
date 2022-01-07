using gpm.Core.Models;

namespace gpm.Core.Services;

public interface IDataBaseService : IDictionary<string, Package>
{
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

    /// <summary>
    /// Re-create protobuf database
    /// </summary>
    void UpdateSelf();

    /// <summary>
    /// Fetch and Pull git database and update
    /// </summary>
    void FetchAndUpdateSelf();
}
