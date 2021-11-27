using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using gpm.core.Models;
using Microsoft.Extensions.Logging;
using ProtoBuf;

namespace gpm.core.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly ILogger<LibraryService> _loggerService;

        public LibraryService(ILogger<LibraryService> loggerService)
        {
            _loggerService = loggerService;

            Load();

            //TODO check self
        }

        private Dictionary<string, PackageModel> _packages = new();

        #region serialization

        public void Load()
        {
            if (File.Exists(IAppSettings.GetLocalDbFile()))
            {
                try
                {
                    using var file = File.OpenRead(IAppSettings.GetLocalDbFile());
                    _packages = Serializer.Deserialize<Dictionary<string, PackageModel>>(file);
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

        #region methods

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

        private bool IsInstalledInSlot(string key, int slot)
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


        /// <summary>
        /// Uninstalls a package from the system by slot
        /// </summary>
        /// <param name="package"></param>
        /// <param name="slotIdx"></param>
        public bool UninstallPackage(Package package, int slotIdx = 0)
        {
            if (!TryGetValue(package.Id, out var model))
            {
                return false;
            }

            if (!model.Slots.TryGetValue(slotIdx, out var slot))
            {
                _loggerService.LogWarning("[{Package}] No package installed in slot {SlotIdx}", package,
                    slotIdx.ToString());
                return true;
            }

            _loggerService.LogInformation("[{Package}] Removing package from slot {SlotIdx}", package,
                slotIdx.ToString());

            var files = slot.Files
                .Select(x => x.Name)
                .ToList();
            var failed = new List<string>();

            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    _loggerService.LogWarning("[{Package}] Could not find file {File} to delete. Skipping", package,
                        file);
                    continue;
                }

                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    _loggerService.LogError(e, "[{Package}] Could not delete file {File}. Skipping", package, file);
                    failed.Add(file);
                }
            }

            // remove deploy manifest from library
            model.Slots.Remove(slotIdx);

            // TODO: remove cached files as well?
            Save();

            if (failed.Count == 0)
            {
                _loggerService.LogDebug("[{Package}] Successfully removed package", package);
                return true;
            }

            _loggerService.LogWarning("[{Package}] Partially removed package. Could not delete:", package);
            foreach (var fail in failed)
            {
                _loggerService.LogWarning("Filename: {File}", fail);
            }

            return false;
        }
    }
}
