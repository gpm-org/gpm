using System.Collections;
using System.Diagnostics.CodeAnalysis;
using gpm.Core.Models;
using ProtoBuf;

namespace gpm.Core.Services;

public sealed class LibraryService : ILibraryService
{
    private Dictionary<string, PackageModel> _packages = new();

    public LibraryService()
    {
        Load();
    }

    #region serialization

    public void Load()
    {
        if (File.Exists(IAppSettings.GetLocalDbFile()))
        {
            try
            {
                using var file = File.OpenRead(IAppSettings.GetLocalDbFile());
                _packages = Serializer.Deserialize<Dictionary<string, PackageModel>>(file);

                // check self

            }
            catch (Exception)
            {
                _packages = new Dictionary<string, PackageModel>();
            }
        }
        else
        {
            _packages = new Dictionary<string, PackageModel>();
        }
    }

    public void Save()
    {
        try
        {
            using var file = File.Create(IAppSettings.GetLocalDbFile());
            Serializer.Serialize(file, _packages);
        }
        catch (Exception)
        {
            if (File.Exists(IAppSettings.GetLocalDbFile()))
            {
                File.Delete(IAppSettings.GetLocalDbFile());
            }
            throw;
        }
    }

    #endregion

    #region IDictionary

    public void Add(string key, PackageModel value) => _packages.Add(key, value);

    public bool ContainsKey(string key) => _packages.ContainsKey(key);

    bool IDictionary<string, PackageModel>.Remove(string key) => _packages.Remove(key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out PackageModel value)
    {
        if (_packages.TryGetValue(key, out var innerValue))
        {
            value = innerValue;
            return true;
        }

        value = null;
        return false;
    }

    public PackageModel this[string key]
    {
        get => _packages[key];
        set => _packages[key] = value;
    }

    public ICollection<string> Keys => _packages.Keys;

    public ICollection<PackageModel> Values => _packages.Values;

    public IEnumerator<KeyValuePair<string, PackageModel>> GetEnumerator() => _packages.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_packages).GetEnumerator();

    public void Add(KeyValuePair<string, PackageModel> item) => _packages.Add(item.Key, item.Value);

    public void Clear() => _packages.Clear();

    public bool Contains(KeyValuePair<string, PackageModel> item) => _packages.Contains(item);
    public void CopyTo(KeyValuePair<string, PackageModel>[] array, int arrayIndex)
        => ((ICollection<KeyValuePair<string, PackageModel>>)_packages).CopyTo(array, arrayIndex);

    public bool Remove(KeyValuePair<string, PackageModel> item) => _packages.Remove(item.Key);

    public int Count => _packages.Count;
    public bool IsReadOnly => ((ICollection<KeyValuePair<string, PackageModel>>)_packages).IsReadOnly;

    #endregion

    #region methods

    public bool TryGetDefaultSlot(Package package, [NotNullWhen(true)] out SlotManifest? slot)
        => TryGetDefaultSlot(package.Id, out slot);

    public bool TryGetDefaultSlot(string key, [NotNullWhen(true)] out SlotManifest? slot)
    {
        slot = null;
        if (!TryGetValue(key, out var model))
        {
            return false;
        }

        slot = model.Slots.FirstOrDefault(x => x.Value.IsDefault.HasValue && x.Value.IsDefault.Value == true).Value;
        return slot is not null;
    }


    public PackageModel GetOrAdd(Package package)
    {
        var key = package.Id;
        if (!_packages.ContainsKey(key))
        {
            _packages.Add(key, new PackageModel(key));
        }

        return _packages[key];
    }

    public bool IsInstalled(Package package) => IsInstalled(package.Id);

    public bool IsInstalled(string key) => ContainsKey(key) && this[key].Slots.Any();

    public bool IsInstalledInSlot(Package package, int slot) => IsInstalledInSlot(package.Id, slot);

    public bool IsInstalledInSlot(string key, int slot)
    {
        if (!TryGetValue(key, out var model))
        {
            return false;
        }
        if (!model.Slots.ContainsKey(slot))
        {
            return false;
        }
        return model.Slots.TryGetValue(slot, out var manifest) && manifest.Files.Any();
    }

    public bool IsInstalledAtLocation(Package package, string path, [NotNullWhen(true)] out int? idx) =>
        IsInstalledAtLocation(package.Id, path, out idx);

    public bool IsInstalledAtLocation(string key, string path, [NotNullWhen(true)] out int? idx)
    {
        idx = null;
        if (!TryGetValue(key, out var model))
        {
            return false;
        }

        foreach (var (i, value) in model.Slots)
        {
            if (value.FullPath is not null && value.FullPath.Equals(path) && value.Files.Any())
            {
                idx = i;
                return true;
            }
        }

        return false;
    }

    #endregion
}
