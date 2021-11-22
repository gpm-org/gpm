using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using gpm.core.Models;
using gpm.core.Services;

namespace gpm.core.Util
{
    public interface IDeploymentService
    {
        /// <summary>
        /// Installs a package from the cache location by version and exact filename
        /// </summary>
        /// <param name="package"></param>
        /// <param name="version"></param>
        /// <param name="releaseFilename"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void InstallPackageFromCache(Package package, string version, string releaseFilename);
    }

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
        /// <param name="releaseFilename"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void InstallPackageFromCache(Package package, string version, string releaseFilename)
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
        public DeployPackageManifest DeploySingleFile(string sourceFileName, string destinationFileName,
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
