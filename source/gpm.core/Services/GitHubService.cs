using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using gpm.core.Models;
using Octokit;

namespace gpm.core.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _gitHubClient = new(new ProductHeaderValue("gpm"));

        private static readonly HttpClient s_client = new();

        private readonly ILibraryService _libraryService;
        private readonly ILoggerService _loggerService;

        public GitHubService(ILibraryService libraryService, ILoggerService loggerService)
        {
            _libraryService = libraryService;
            _loggerService = loggerService;
        }

        public async Task<bool> InstallLatestReleaseAsync(Package package)
        {
            IReadOnlyList<Release>? releases;
            try
            {
                releases = await _gitHubClient.Repository.Release.GetAll(package.RepoOwner, package.RepoName);
            }
            catch (Exception e)
            {
                _loggerService.Error(e);
                throw;
            }
            if (releases == null || !releases.Any())
            {
                return false;
            }


            // install latest
            var latest = releases[0];
            var idx = package.AssetIndex;

            var zip = latest.Assets[idx];
            if (zip is null)
            {
                return false;
            }
            var url = latest.Assets[idx].BrowserDownloadUrl;
            var filename = latest.Assets[idx].Name;
            var version = latest.TagName;
            
            var cacheFile = await DownloadAssetToCache(package, filename, version);
            if (cacheFile == null)
            {
                return false;
            }

            var info = await InstallAsset(package, cacheFile, version);
            if (info == null)
            {
                return false;
            }

            // udpate current info in local db
            if (_libraryService.Contains(package.Id))
            {
                // update version?
            }
            else
            {
                // add
                var model = new PackageModel(package.Id)
                {
                    InstalledVersion = version,
                    Url = url,
                };
                model.InstalledVersions.Add(version, info);
                _libraryService.Packages.Add(version, model);
            }
            _libraryService.Save();

            return true;
        }

        private static async Task<string?> DownloadAssetToCache(Package package, string filename, string version)
        {
            var libdir = Path.Combine(IAppSettings.GetCacheFolder(), $"{package.Id}", $"{version}");
            if (!Directory.Exists(libdir))
            {
                Directory.CreateDirectory(libdir);
            }
            var path = Path.Combine(libdir, filename);

            try
            {
                var uri = new Uri(package.Url);
                var response = await s_client.GetAsync(uri);

                response.EnsureSuccessStatusCode();

                using var fs = new FileStream(path, System.IO.FileMode.Create, FileAccess.Write);
                await response.Content.CopyToAsync(fs);

                return path;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);

                return null;
            }
        }

        private async Task<VersionInfo?> InstallAsset(Package package, string path, string version)
        {
            // TODO: unzip
            if (package.ContentType is null)
            {
                var extension = Path.GetExtension(path).ToLower();
                switch (extension)
                {
                    case ".zip":
                        return await ExtractZipArchiveAsync(package, path, version);
                    default:
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
                        return await ExtractZipArchiveAsync(package, path, version);
                    case EContentType.SevenZipArchive:
                        break;
                }
            }

            return null;
        }

        private async Task<VersionInfo?> ExtractZipArchiveAsync(Package package, string zipPath, string version)
        {
            var extension = Path.GetExtension(zipPath).ToLower();
            if (extension != ".zip")
            {
                throw new ArgumentException(nameof(zipPath));
            }

            // extract zipfile
            // get the files in the zip archive
            var files = new List<string>();
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // check if folder
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        continue;
                    }
                    files.Add(entry.FullName);
                }
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

                string appFolder = Path.Combine(IAppSettings.GetLibraryFolder(), package.Id);
                string destinationDirectoryName = Path.Combine(appFolder, version);
                ZipFile.ExtractToDirectory(zipPath, destinationDirectoryName, true);
            }
            catch (Exception e)
            {
                _loggerService.Error(e);
                return null;
            }

            await Task.Delay(1);

            return new VersionInfo()
            {
                DeployedFiles = files.ToArray(),
            };
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
