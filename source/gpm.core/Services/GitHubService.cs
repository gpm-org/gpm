using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Nito.AsyncEx;
using System.Threading.Tasks;
using gpm.core.Exceptions;
using gpm.core.Extensions;
using gpm.core.Models;
using Octokit;
using gpm.core.Util;

namespace gpm.core.Services
{
    /// <summary>
    /// A service class to handle requests to github
    /// </summary>
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _gitHubClient = new(new ProductHeaderValue("gpm"));
        private static readonly HttpClient s_client = new();
        private readonly AsyncLock _loadingLock = new();

        private readonly ILibraryService _libraryService;
        private readonly ILoggerService _loggerService;
        private readonly IDeploymentService _deploymentService;

        public GitHubService(ILibraryService libraryService,
            ILoggerService loggerService,
            IDeploymentService deploymentService)
        {
            _libraryService = libraryService;
            _loggerService = loggerService;
            _deploymentService = deploymentService;
        }

        /// <summary>
        /// Fetch all github releases for a given package.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<Release>?> GetReleasesForPackage(Package package)
        {
            using var ssc = new ScopedStopwatch();

            IReadOnlyList<Release>? releases;

            // get releases from github repo
            using (await _loadingLock.LockAsync())
            {
                try
                {
                    releases = await _gitHubClient.Repository.Release.GetAll(package.RepoOwner, package.RepoName);
                }
                catch (Exception e)
                {
                    _loggerService.Error(e);
                    releases = null;
                }
            }

            return releases;
        }

        /// <summary>
        /// Check if a release exists that
        /// </summary>
        /// <param name="package"></param>
        /// <param name="releases"></param>
        /// <param name="installedVersion"></param>
        /// <returns></returns>
        public bool IsUpdateAvailable(Package package, IReadOnlyList<Release>? releases, string installedVersion)
        {
            using var ssc = new ScopedStopwatch();

            if (releases is null || !releases.Any())
            {
                _loggerService.Warning($"No releases found for package {package.Id}");
                return false;
            }
            if (!releases.Any(x => x.TagName.Equals(installedVersion)))
            {
                _loggerService.Warning($"No releases found with version {installedVersion} for package {package.Id}");
                return false;
            }

            // idx 0 is latest release
            if (!releases[0].TagName.Equals(installedVersion))
            {
                return true;
            }

            _loggerService.Information($"Latest release already installed.");
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
                _loggerService.Warning($"No release found for version {requestedVersion}");
                return false;
            }

            // get correct release asset
            // TODO support multiple asset files?

            // TODO support asset get logic

            var idx = package.AssetIndex;
            var asset = release.Assets[idx];
            if (asset is null)
            {
                _loggerService.Warning($"No release asset found for version {requestedVersion} and index {idx.ToString()}");
                return false;
            }

            // get download paths
            var version = release.TagName;

            // check if version is already installed
            if (_libraryService.TryGetValue(package.Id, out var model) && _libraryService.IsInstalledInSlot(package, slot))
            {
                var slotManifest = model.Slots[slot];
                var installedVersion = slotManifest.Version;
                if (installedVersion is not null && installedVersion.Equals(version))
                {
                    _loggerService.Information($"[{package.Id}] Version {version} already installed.");
                    return false;
                }
            }

            ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(version);
            if (!await DownloadAssetToCache(package, asset, version))
            {
                _loggerService.Warning($"Failed to download package {package.Id}");
                return false;
            }

            // install asset
            var releaseFilename = asset.Name;
            ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(releaseFilename);
            if (! _deploymentService.InstallPackageFromCache(package, version, slot))
            {
                _loggerService.Warning($"Failed to install package {package.Id}");
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
                _loggerService.Information($"Asset exists in cache: {assetCacheFile}. Using cached file.");
                return true;
            }

            try
            {
                var url = asset.BrowserDownloadUrl;
                ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(url);

                _loggerService.Information($"Downloading asset from {url} ...");

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

                _loggerService.Success($"Downloaded asset {releaseFilename} with hash {HashUtil.BytesToString(sha)}.");
                _loggerService.Information($"Saving file to local cache: {assetCacheFile}.");

                // cache manifest
                var cacheManifest = new CacheManifest()
                {
                    Files = new[]
                    {
                        new HashedFile(releaseFilename, sha, size)
                    }
                };

                var model = _libraryService.GetOrAdd(package);
                model.CacheData.AddOrUpdate(version, cacheManifest);
                _libraryService.Save();

                return true;
            }
            catch (HttpRequestException httpRequestException)
            {
                _loggerService.Error(httpRequestException);
                return false;
            }
            catch (Exception e)
            {
                _loggerService.Error(e);
                return false;
            }

            bool CheckIfCachedFileExists()
            {
                if (!File.Exists(assetCacheFile))
                {
                    return false;
                }

                if (!_libraryService.TryGetValue(package.Id, out var existingModel))
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
