using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        public GitHubService(ILibraryService libraryService, ILoggerService loggerService)
        {
            _libraryService = libraryService;
            _loggerService = loggerService;
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

            InstallPackageFromCache(package, releaseTagName, releaseFilename);

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

        /// <summary>
        /// Installs a package from the cache location by version and exact filename
        /// </summary>
        /// <param name="package"></param>
        /// <param name="version"></param>
        /// <param name="releaseFilename"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void InstallPackageFromCache(Package package, string version, string releaseFilename)
        {
            _loggerService.Info($"Installing {package.Id} from cache...");

            var packageCacheFolder = Path.Combine(IAppSettings.GetCacheFolder(), $"{package.Id}", $"{version}");
            if (!Directory.Exists(packageCacheFolder))
            {
                throw new DirectoryNotFoundException();
            }
            var assetCachePath = Path.Combine(packageCacheFolder, releaseFilename);

            //TODO: support multiple files here
            string packageLibraryDir = Path.Combine(IAppSettings.GetLibraryFolder(), package.Id, version);
            if (!Directory.Exists(packageLibraryDir))
            {
                Directory.CreateDirectory(packageLibraryDir);
            }
            DeployPackageManifest? info = null;
            if (package.ContentType is null)
            {
                // TODO: multiple archive support
                var extension = Path.GetExtension(assetCachePath).ToLower();
                switch (extension)
                {
                    case ".zip":
                        // TODO: installation instructions
                        info = ExtractZipArchiveTo(assetCachePath, packageLibraryDir);
                        break;
                    default:
                        // treat as single file
                        // TODO: installation instructions
                        // move from cache to library
                        var assetDestinationPath = Path.Combine(packageLibraryDir, releaseFilename);
                        info = DeploySingleFile(assetCachePath, assetDestinationPath);
                        break;
                }
            }
            else
            {
                switch (package.ContentType)
                {
                    case EContentType.SingleFile:
                        break;
                    case EContentType.ZipArchive:
                        info = ExtractZipArchiveTo(assetCachePath, packageLibraryDir);
                        break;
                    case EContentType.SevenZipArchive:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(package.ContentType), "Invalid Package content type.");
                }
            }

            ArgumentNullException.ThrowIfNull(info);

            // update library
            var model = _libraryService.GetOrAdd(package);
            model.AddOrUpdateManifest(version, info);
            model.LastInstalledVersion = version;
            _libraryService.Save();

        }

        /// <summary>
        /// Deploys a single file to its install destination
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destinationFileName"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        private DeployPackageManifest DeploySingleFile(string sourceFileName, string destinationFileName,
            bool overwrite = true)
        {
            File.Copy(sourceFileName, destinationFileName, overwrite);

            var info = new DeployPackageManifest()
            {
                Files = new[]
                {
                    new HashedFile( destinationFileName, null, null)
                }
            };

            _loggerService.Success($"Installed package to {destinationFileName}.");

            return info;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sourceArchiveFileName"></param>
        /// <param name="destinationDirectoryName"></param>
        /// <param name="overwriteFiles"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private DeployPackageManifest? ExtractZipArchiveTo(string sourceArchiveFileName, string destinationDirectoryName,
            bool overwriteFiles = true)
        {
            var extension = Path.GetExtension(sourceArchiveFileName).ToLower();
            if (extension != ".zip")
            {
                throw new ArgumentException(null, nameof(sourceArchiveFileName));
            }

            // extract zipFile
            // get the files in the zip archive
            var files = new List<string>();
            using (ZipArchive archive = ZipFile.OpenRead(sourceArchiveFileName))
            {
                files.AddRange(from entry in archive.Entries
                    where !string.IsNullOrEmpty(entry.Name)
                    select entry.FullName);
            }

            // check for conflicts with existing files
            //var conflicts = files.Where(x => File.Exists(Path.Combine(_settingsService.GetGameRootPath(), x)));
            //if (conflicts.Any())
            //{
            //    // ask user
            //    switch (await _interactionService.ShowConfirmation($"The following files will be overwritten, continue?\r\n\r\n {string.Join("\r\n", conflicts)}", "Install Mod"))
            //    {
            //        case WMessageBoxResult.None:
            //        case WMessageBoxResult.Cancel:
            //        case WMessageBoxResult.No:
            //            return null;
            //    }
            //}

            // extract to
            try
            {
                //TODO parse destinationDirectoryName
                // TODO package install directories

                ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, overwriteFiles);

                _loggerService.Success($"Installed {sourceArchiveFileName} to {destinationDirectoryName}.");

                return new DeployPackageManifest() { Files = files
                    .Select(x => new HashedFile(x, null, null))
                    .ToArray(), };
            }
            catch (Exception e)
            {
                _loggerService.Error(e);
                return null;
            }
        }


        //    private async Task DownloadUpdateAsync(Manifest manifest)
        //    {
        //        var latestVersion = manifest.Version;

        //        if (!b)
        //        {
        //            return true;
        //        }

        //        using (var wc = new WebClient())
        //        {
        //            var dlObservable = Observable.FromEventPattern<DownloadProgressChangedEventHandler, DownloadProgressChangedEventArgs>(
        //                handler => wc.DownloadProgressChanged += handler,
        //                handler => wc.DownloadProgressChanged -= handler);
        //            var dlCompleteObservable = Observable.FromEventPattern<AsyncCompletedEventHandler, AsyncCompletedEventArgs>(
        //                handler => wc.DownloadFileCompleted += handler,
        //                handler => wc.DownloadFileCompleted -= handler);

        //            _ = dlObservable
        //                .Select(_ => (double)_.EventArgs.ProgressPercentage)
        //                .Subscribe(d =>
        //                {
        //                    Report(d / 100);
        //                });

        //            _ = dlCompleteObservable
        //                .Select(_ => _.EventArgs)
        //                .Subscribe(c =>
        //                {
        //                    OnDownloadCompletedCallback(c, manifest, type);
        //                });

        //            var uri = new Uri($"{GetUpdateUri().TrimEnd('/')}/{manifest.Get(type).Key}");
        //            var physicalPath = Path.Combine(Path.GetTempPath(), manifest.Get(type).Key);
        //            wc.DownloadFileAsync(uri, physicalPath);
        //        }
        //        await Task.CompletedTask;
        //    }

        //    private void OnDownloadCompletedCallback(AsyncCompletedEventArgs e, Manifest manifest, EIncludedFiles type)
        //    {
        //        if (e.Cancelled)
        //        {
        //            Console.WriteLine("File download cancelled.");
        //        }

        //        if (e.Error != null)
        //        {
        //            Console.WriteLine(e.Error);
        //        }

        //        // check downloaded file
        //        var physicalPath = new FileInfo(Path.Combine(Path.GetTempPath(), manifest.Get(type).Key));
        //        if (physicalPath.Exists)
        //        {
        //            using (var mySha256 = SHA256.Create())
        //            {
        //                var hash = Helpers.HashFile(physicalPath, mySha256);
        //                if (manifest.Get(type).Value.Equals(hash))
        //                {
        //                    HandleUpdateFromFile(physicalPath);
        //                }
        //                else
        //                {
        //                    Console.WriteLine("Downloaded file does not match expected file.");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine("File download failed.");
        //        }
        //    }
        //
    }
}
