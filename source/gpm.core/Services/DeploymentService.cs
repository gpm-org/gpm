using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using gpm.core.Extensions;
using gpm.core.Models;
using gpm.core.Util;

namespace gpm.core.Services
{
    /// <summary>
    /// A service class for file deployments
    /// </summary>
    public class DeploymentService : IDeploymentService
    {
        private readonly ILibraryService _libraryService;
        private readonly ILoggerService _loggerService;

        public DeploymentService(ILibraryService libraryService, ILoggerService loggerService)
        {
            _libraryService = libraryService;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Installs a package from the cache location by version and exact filename
        /// </summary>
        /// <param name="package"></param>
        /// <param name="version"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool InstallPackageFromCache(Package package, string version, int slot = 0)
        {
            using var ssc = new ScopedStopwatch();

            _loggerService.Information($"[{package.Id}] Installing from cache...");

            // checks
            var packageCacheFolder = Path.Combine(IAppSettings.GetCacheFolder(), $"{package.Id}", $"{version}");
            if (!Directory.Exists(packageCacheFolder))
            {
                throw new DirectoryNotFoundException();
            }
            if (!_libraryService.TryGetValue(package.Id, out var model))
            {
                // throw because the cache manifest needs to exist from a previous step
                throw new KeyNotFoundException();
            }
            if (!model.CacheData.TryGetValue(version, out var cacheManifest))
            {
                // throw because the cache manifest needs to exist from a previous step
                throw new KeyNotFoundException();
            }
            if (cacheManifest.Files is null)
            {
                _loggerService.Warning($"[{package.Id}] No files to install.");
                return false;
            }
            if (cacheManifest.Files.Length < 1)
            {
                _loggerService.Warning($"[{package.Id}] No files to install.");
                return false;
            }

            //TODO: support multiple files
            var assetCacheFile = cacheManifest.Files.First().Name;
            var assetCachePath = Path.Combine(packageCacheFolder, assetCacheFile);

            // get or create new slot
            var slotManifest = model.Slots.GetOrAdd(slot);

            // check if version is already installed
            var installedVersion = slotManifest.Version;
            if (installedVersion is not null && installedVersion.Equals(version))
            {
                _loggerService.Information($"[{package.Id}] Version {version} already installed.");
                return false;
            }

            // get destination path
            if (slotManifest.FullPath is null)
            {
                // TODO: installation instructions

                slotManifest.FullPath = Path.Combine(IAppSettings.GetLibraryFolder(), package.Id);
                if (!Directory.Exists(slotManifest.FullPath))
                {
                    Directory.CreateDirectory(slotManifest.FullPath);
                }
            }
            var destinationDir = slotManifest.FullPath;


            // TODO ask or overwrite

            List<HashedFile>? installedFiles;
            if (package.ContentType is null)
            {
                var extension = Path.GetExtension(assetCachePath).ToLower();
                switch (extension)
                {
                    case ".zip":
                        installedFiles = ExtractZipArchiveTo(assetCachePath, destinationDir);
                        break;
                    default:
                        // treat as single file
                        var releaseFilename = Path.GetFileName(assetCachePath);
                        var assetDestinationPath = Path.Combine(destinationDir, releaseFilename);
                        installedFiles = DeploySingleFile(assetCachePath, assetDestinationPath);
                        break;
                }
            }
            else
            {
                switch (package.ContentType)
                {
                    case EContentType.SingleFile:
                        var releaseFilename = Path.GetFileName(assetCachePath);
                        var assetDestinationPath = Path.Combine(destinationDir, releaseFilename);
                        installedFiles = DeploySingleFile(assetCachePath, assetDestinationPath);
                        break;
                    case EContentType.ZipArchive:
                        installedFiles = ExtractZipArchiveTo(assetCachePath, destinationDir);
                        break;
                    case EContentType.SevenZipArchive:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(package.ContentType), "Invalid Package content type.");
                }
            }

            if (installedFiles is null)
            {
                _loggerService.Warning($"[{package.Id}] No files installed. Aborting.");
                return false;
            }

            // update library
            slotManifest.Version = version;
            slotManifest.Files = installedFiles;
            _libraryService.Save();
            return true;
        }

        /// <summary>
        /// Deploys a single file to its install destination
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destinationFileName"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        private List<HashedFile> DeploySingleFile(string sourceFileName, string destinationFileName,
            bool overwrite = true)
        {
            // TODO: conflicts

            File.Copy(sourceFileName, destinationFileName, overwrite);

            _loggerService.Success($"Installed package to {destinationFileName}.");

            return new List<HashedFile> { new HashedFile(destinationFileName, null, null) };
        }

        /// <summary>
        /// Extracts a zip archive to a given destination directory
        /// </summary>
        /// <param name="sourceArchiveFileName"></param>
        /// <param name="destinationDirectoryName"></param>
        /// <param name="overwriteFiles"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private List<HashedFile>? ExtractZipArchiveTo(string sourceArchiveFileName, string destinationDirectoryName,
            bool overwriteFiles = true)
        {
            var extension = Path.GetExtension(sourceArchiveFileName).ToLower();
            if (extension != ".zip")
            {
                throw new ArgumentException(null, nameof(sourceArchiveFileName));
            }

            // get the files in the zip archive
            var files = new List<string>();
            using (ZipArchive archive = ZipFile.OpenRead(sourceArchiveFileName))
            {
                files.AddRange(from entry in archive.Entries
                    where !string.IsNullOrEmpty(entry.Name)
                    select entry.FullName);
            }

            // TODO: conflicts
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
                ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, overwriteFiles);
            }
            catch (Exception e)
            {
                _loggerService.Error(e);
                return null;
            }

            _loggerService.Success($"Installed {sourceArchiveFileName} to {destinationDirectoryName}.");

            return files
                .Select(x => new HashedFile(x, null, null))
                .ToList();
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
