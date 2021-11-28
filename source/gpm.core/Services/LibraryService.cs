using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using gpm.core.Exceptions;
using gpm.core.Extensions;
using gpm.core.Models;
using gpm.core.Util;
using Octokit;
using Serilog;
using ProtoBuf;

namespace gpm.core.Services
{
    public sealed class LibraryService : ILibraryService
    {
        private readonly IDeploymentService _deploymentService;
        private static readonly HttpClient s_client = new();

        private Dictionary<string, PackageModel> _packages = new();

        public LibraryService(IDeploymentService deploymentService)
        {
            _deploymentService = deploymentService;

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
                Log.Warning("[{Package}] No package installed in slot {SlotIdx}", package,
                    slotIdx.ToString());
                return true;
            }

            Log.Information("[{Package}] Removing package from slot {SlotIdx}", package,
                slotIdx.ToString());

            var files = slot.Files
                .Select(x => x.Name)
                .ToList();
            var failed = new List<string>();

            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    Log.Warning("[{Package}] Could not find file {File} to delete. Skipping", package,
                        file);
                    continue;
                }

                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Log.Error(e, "[{Package}] Could not delete file {File}. Skipping", package, file);
                    failed.Add(file);
                }
            }

            // remove deploy manifest from library
            model.Slots.Remove(slotIdx);

            // TODO: remove cached files as well?
            Save();

            if (failed.Count == 0)
            {
                Log.Debug("[{Package}] Successfully removed package", package);
                return true;
            }

            Log.Warning("[{Package}] Partially removed package. Could not delete:", package);
            foreach (var fail in failed)
            {
                Log.Warning("Filename: {File}", fail);
            }

            return false;
        }

        /// <summary>
        /// Download and install an asset file from a Github repo.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="releases"></param>
        /// <param name="requestedVersion"></param>
        /// <param name="slot"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public async Task<bool> InstallReleaseAsync(
            Package package,
            IReadOnlyList<Release> releases,
            string? requestedVersion,
            int slot = 0)
        {
            using var ssc = new ScopedStopwatch();

            // get correct release
            var release = string.IsNullOrEmpty(requestedVersion)
                ? releases[0] //latest
                : releases.FirstOrDefault(x => x.TagName.Equals(requestedVersion));

            if (release == null)
            {
                Log.Warning("No release found for version {RequestedVersion}", requestedVersion);
                return false;
            }

            // get correct release asset
            // TODO support multiple asset files?

            // TODO support asset get logic

            var idx = package.AssetIndex;
            var asset = release.Assets[idx];
            if (asset is null)
            {
                Log.Warning("No release asset found for version {RequestedVersion} and index {Idx}",
                    requestedVersion, idx.ToString());
                return false;
            }

            // get download paths
            var version = release.TagName;

            // check if version is already installed
            if (TryGetValue(package.Id, out var model) && IsInstalledInSlot(package, slot))
            {
                var slotManifest = model.Slots[slot];
                var installedVersion = slotManifest.Version;
                if (installedVersion is not null && installedVersion.Equals(version))
                {
                    Log.Information("[{Package}] Version {Version} already installed", package, version);
                    return false;
                }
            }

            ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(version);
            if (!await DownloadAssetToCache(package, asset, version))
            {
                Log.Warning("Failed to download package {Package}", package);
                return false;
            }

            // install asset
            var releaseFilename = asset.Name;
            ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(releaseFilename);
            if (! _deploymentService.InstallPackageFromCache(package, version, slot))
            {
                Log.Warning("Failed to install package {Package}", package);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Downloads a given release asset from GitHub and saves it to the cache location
        /// Creates a CacheManifest inside the library
        /// </summary>
        /// <param name="package"></param>
        /// <param name="asset"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        private async Task<bool> DownloadAssetToCache(Package package, ReleaseAsset asset, string version)
        {
            using var ssc = new ScopedStopwatch();

            var releaseFilename = asset.Name;
            ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(releaseFilename);

            var packageCacheFolder = Path.Combine(IAppSettings.GetCacheFolder(), $"{package.Id}", $"{version}");
            string assetCacheFile = Path.Combine(packageCacheFolder, releaseFilename);

            // check if already exists
            if (CheckIfCachedFileExists())
            {
                Log.Information("Asset exists in cache: {AssetCacheFile}. Using cached file",
                    assetCacheFile);
                return true;
            }

            var url = asset.BrowserDownloadUrl;
            ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(url);
            try
            {
                Log.Information("Downloading asset from {Url} ...", url);

                var response = await s_client.GetAsync(new Uri(url));
                response.EnsureSuccessStatusCode();

                if (!Directory.Exists(packageCacheFolder))
                {
                    Directory.CreateDirectory(packageCacheFolder);
                }

                await using var ms = new MemoryStream();
                await response.Content.CopyToAsync(ms);

                // sha and size the ms
                var size = ms.Length;
                var sha = HashUtil.Sha512Bytes(ms);

                // write to file
                ms.Seek(0, SeekOrigin.Begin);
                await using var fs = new FileStream(assetCacheFile, System.IO.FileMode.Create, FileAccess.Write);
                await ms.CopyToAsync(fs);

                Log.Debug("Downloaded asset {ReleaseFilename} with hash {Hash}", releaseFilename,
                    HashUtil.BytesToString(sha));
                Log.Information("Saving file to local cache: {AssetCacheFile}", assetCacheFile);

                // cache manifest
                var cacheManifest = new CacheManifest()
                {
                    Files = new[]
                    {
                        new HashedFile(releaseFilename, sha, size)
                    }
                };

                var model = GetOrAdd(package);
                model.CacheData.AddOrUpdate(version, cacheManifest);
                Save();

                return true;
            }
            catch (HttpRequestException httpRequestException)
            {
                Log.Error(httpRequestException, "Downloading asset from {Url} failed", url);
                return false;
            }
            catch (Exception e)
            {
                Log.Error(e, "Downloading asset from {Url} failed", url);
                return false;
            }

            bool CheckIfCachedFileExists()
            {
                if (!File.Exists(assetCacheFile))
                {
                    return false;
                }

                if (!TryGetValue(package.Id, out var existingModel))
                {
                    return false;
                }

                var cacheManifest = existingModel.CacheData.GetOptional(version);
                if (!cacheManifest.HasValue)
                {
                    return false;
                }
                if (cacheManifest.Value.Files is null)
                {
                    return false;
                }

                // if the cache manifest contains a file with this name
                var fileInCache = cacheManifest.Value.Files
                    .FirstOrDefault(x => Path.Combine(packageCacheFolder, x.Name).Equals(assetCacheFile));
                if (fileInCache is { })
                {
                    // size and hash
                    using var fs = new FileStream(assetCacheFile, System.IO.FileMode.Open, FileAccess.Read);
                    var size = fs.Length;
                    var sha = HashUtil.Sha512Bytes(fs);
                    if (fileInCache.Sha512 != null && fileInCache.Sha512.SequenceEqual(sha) && fileInCache.Size == size)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
