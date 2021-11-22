using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Nito.AsyncEx;
using System.Threading.Tasks;
using gpm.core.Exceptions;
using gpm.core.Models;
using Octokit;
using gpm.core.Util;

namespace gpm.core.Services
{
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
        /// Check if a release exists that 
        /// </summary>
        /// <param name="package"></param>
        /// <param name="currentVersion"></param>
        /// <returns></returns>
        public async Task<bool> IsUpdateAvailable(Package package, string? currentVersion)
        {
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
            if (releases == null || !releases.Any())
            {
                _loggerService.Warning($"No releases found for package {package.Id}");
                return false;
            }

            if (!releases.Any(x => x.TagName.Equals(currentVersion)))
            {
                _loggerService.Warning($"No releases found with version {currentVersion} for package {package.Id}");
                return false;
            }

            // idx 0 is latest release
            if (releases[0].TagName.Equals(currentVersion))
            {
                _loggerService.Info($"Latest release already installed.");
                return false;
            }
            
            return true;
        }


        /// <summary>
        /// Download and install an asset file from a Github repo.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="version"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public async Task<bool> InstallReleaseAsync(Package package, string? version)
        {
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
            if (releases == null || !releases.Any())
            {
                _loggerService.Warning($"No releases found for package {package.Id}");
                return false;
            }

            // get correct release
            var release = string.IsNullOrEmpty(version)
                ? releases[0]
                : releases.FirstOrDefault(x => x.TagName.Equals(version));

            if (release == null)
            {
                _loggerService.Warning($"No release found for version {version}");
                return false;
            }

            // get correct release asset
            // TODO support multiple files?
            // TODO support logic
            var idx = package.AssetIndex;
            var asset = release.Assets[idx];
            if (asset is null)
            {
                _loggerService.Warning($"No release asset found for version {version} and index {idx.ToString()}");
                return false;
            }

            // get download paths
            var releaseTagName = release.TagName;
            ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(releaseTagName);

            // download asset to library
            var isAssetDownloaded = await DownloadAssetToCache(package, asset, releaseTagName);
            if (!isAssetDownloaded)
            {
                _loggerService.Error($"Failed to download package {package.Id}");
                return false;
            }

            // install asset
            var releaseFilename = asset.Name;
            ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(releaseFilename);

            _deploymentService.InstallPackageFromCache(package, releaseTagName, releaseFilename);

            return true;
        }

        /// <summary>
        /// Downloads a given release asset from GitHub and saves it to the cache location
        /// </summary>
        /// <param name="package"></param>
        /// <param name="asset"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        private async Task<bool> DownloadAssetToCache(Package package, ReleaseAsset asset, string version)
        {
            var releaseFilename = asset.Name;
            ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(releaseFilename);

            var packageCacheFolder = Path.Combine(IAppSettings.GetCacheFolder(), $"{package.Id}", $"{version}");
            string assetCacheFile = Path.Combine(packageCacheFolder, releaseFilename);

            // check if already exists
            if (CheckIfCachedFileExists())
            {
                _loggerService.Info($"Asset exists in cache: {assetCacheFile}. Using cached file.");
                return true;
            }

            try
            {
                var url = asset.BrowserDownloadUrl;
                ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(url);

                _loggerService.Info($"Downloading asset from {url} ...");

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
                _loggerService.Info($"Saving file to local cache: {assetCacheFile}.");

                //TODO cache manifest
                // cache manifest
                var manifest = new CachePackageManifest()
                {
                    Files = new[]
                    {
                        new HashedFile(releaseFilename, sha, size)
                    }
                };

                var model = _libraryService.GetOrAdd(package);
                model.AddOrUpdateManifest(version, manifest);
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
                var existingModel = _libraryService.Lookup(package.Id);
                if (!existingModel.HasValue)
                {
                    return false;
                }
                var cacheManifest = existingModel.Value.TryGetManifest<CachePackageManifest>(version);
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
