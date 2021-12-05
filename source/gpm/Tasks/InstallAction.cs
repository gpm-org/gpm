using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using gpm.core.Exceptions;
using gpm.core.Extensions;
using gpm.core.Models;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace gpm.Tasks
{
    public static class InstallAction
    {
        /// Examples:
        /// gpm install redscript -g    installs redscript in the default location (global)
        /// gpm install -p PATH         installs redscript in the specified location (global)
        /// gpm install redscript       installs redscript in the current directory (local)
        /// gpm install -g -p PATH      not allowed

        /// <summary>
        /// Update and install a package (optionally with a given version)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="path">The global install directory, can't be combined with -g</param>
        /// <param name="global">Install this package globally in the default location</param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateAndInstall(string name, string version, string path, bool global, IHost host)
        {
            Upgrade.Action(host);

            return await Install(name, version, path, global, host);
        }

        /// <summary>
        /// Install a package (optionally with a given version)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="path">The global install directory, can't be combined with -g</param>
        /// <param name="global">Install this package globally in the default location</param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static async Task<bool> Install(string name, string version, string path, bool global, IHost host)
        {
            var serviceProvider = host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            var gitHubService = serviceProvider.GetRequiredService<IGitHubService>();
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();
            var deploymentService = serviceProvider.GetRequiredService<IDeploymentService>();

            // checks
            if (Ensure.IsNotNullOrEmpty(name, () => Log.Warning($"No package name specified to install.")))
            {
                return false;
            }
            var package = dataBaseService.GetPackageFromName(name);
            if (package is null)
            {
                Log.Warning("[{Package}] Package {Name} not found", package, name);
                return false;
            }
            // get install path
            if (!GetInstallPath(path, global, libraryService, package, out var slotId))
            {
                return false;
            }

            // install package
            Log.Information("[{Package}] Installing package ...", package);
            var releases = await gitHubService.GetReleasesForPackage(package);
            if (releases is null || !releases.Any())
            {
                Log.Warning("No releases found for package {Package}", package);
                return false;
            }

            if (await deploymentService.InstallReleaseAsync(package, releases, version, slotId))
            {
                Log.Information("[{Package}] Package successfully installed", package);
            }
            else
            {
                // clean slots for failed install
                if (libraryService.TryGetValue(package.Id, out var model))
                {
                    model.Slots.Remove(slotId);
                }
                return false;
            }

            return await InstallDependencies(path, global, host, package) || true;
        }

        private static async Task<bool> InstallDependencies(string path, bool global, IHost host, Package package)
        {
            // dependencies
            var dependencies = package.Dependencies;
            if (dependencies is null)
            {
                return true;
            }

            var dependencyResult = true;
            if (dependencies.Length > 0)
            {
                // TODO install dependencies: versions, paths?
                Log.Information("[{Package}] Found {Count} dependencies. Installing...", package, dependencies.Length);
                foreach (var dep in dependencies)
                {
                    dependencyResult = await Install(dep, "", path, global, host);
                }
            }

            if (!dependencyResult)
            {
                Log.Warning("[{Package}] Some dependencies failed to install correctly", package);
            }
            else
            {
                Log.Information("[{Package}] All dependencies installed successfully", package);
            }

            return false;
        }

        private static bool GetInstallPath(string path, bool global, ILibraryService libraryService, Package package,
            out int slotId)
        {
            // check if package is in local library
            // if not it just goes to slot 0
            var model = libraryService.GetOrAdd(package);
            slotId = 0;
            string installPath = "";
            var isPathEmpty = string.IsNullOrEmpty(path);
            switch (global)
            {
                // global & path            not allowed
                case true when !isPathEmpty:
                    Log.Warning(
                        "[{Package}] --global specifies the installation is user wide. Can't be combined with the --path option",
                        package);
                    return false;
                // global & not path        install in default dir
                case true when isPathEmpty:
                // not global & path        install in path
                case false when !isPathEmpty:
                {
                    installPath = isPathEmpty
                        ? Path.Combine(IAppSettings.GetLibraryFolder(), package.Id)
                        : path;
                    if (!Directory.Exists(installPath))
                    {
                        Directory.CreateDirectory(installPath);
                    }
                    break;
                }
                // not global & not path    install in current dir
                case false when isPathEmpty:
                {
                    installPath = Environment.CurrentDirectory;
                    break;
                }
            }

            ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(installPath);

            // install package as a local tool in a specified location
            // check if that path matches any existing slot
            var slotForPath = model.Slots.Values
                .FirstOrDefault(x => x.FullPath != null && x.FullPath.Equals(installPath));
            if (slotForPath is not null && libraryService.IsInstalled(model.Key))
            {
                // is already installed
                // TODO: if it is, return because we should use update or repair
                Log.Warning("[{Package}] Already installed in slot {Path} - Use gpm update or gpm repair",
                    package, installPath);
                return false;
            }

            // if not, add to a new slot2
            foreach (var (key, _) in model.Slots)
            {
                if (key != slotId)
                {
                    break;
                }
                slotId++;
            }

            var slotManifest = model.Slots.GetOrAdd(key: slotId);
            slotManifest.FullPath = installPath;

            return true;
        }
    }
}
