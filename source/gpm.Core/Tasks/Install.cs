using gpm.Core.Exceptions;
using gpm.Core.Extensions;
using gpm.Core.Models;
using gpm.Core.Services;
using Serilog;

namespace gpm.Core.Services;

public partial class TaskService
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
    /// <returns></returns>
    public async Task<bool> UpgradeAndInstall(string name, string version, string path, bool global)
    {
        if (!Upgrade())
        {
            return false;
        }

        return await Install(name, version, path, global);
    }

    /// <summary>
    /// Install a package (optionally with a given version)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="version"></param>
    /// <param name="path">The global install directory, can't be combined with -g</param>
    /// <param name="global">Install this package globally in the default location</param>
    /// <returns></returns>
    public async Task<bool> Install(string name, string version, string path, bool global)
    {
        // checks
        if (Ensure.IsNotNullOrEmpty(name, () => Log.Warning($"No package name specified to install.")))
        {
            return false;
        }
        var package = _dataBaseService.GetPackageFromName(name);
        if (package is null)
        {
            Log.Warning("[{Package}] Package {Name} not found", package, name);
            return false;
        }
        // get install path
        if (!TryGetInstallPath(package, path, global, out var installPath, out var isDefault))
        {
            return false;
        }

        // install package as a local tool in a specified location
        // check if that path matches any existing slot
        var model = _libraryService.GetOrAdd(package);
        var slotForPath = model.Slots.Values
            .FirstOrDefault(x => x.FullPath != null && x.FullPath.Equals(installPath));
        if (slotForPath is not null && _libraryService.IsInstalled(model.Key))
        {
            // is already installed
            // TODO: if it is, return because we should use update or repair
            Log.Warning("[{Package}] Already installed in slot {Path} - Use gpm update or gpm repair",
                package, installPath);
            return false;
        }

        // if not, add to a new slot
        var slotId = 0;
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
        slotManifest.IsDefault = isDefault;

        // install package
        Log.Information("[{Package}] Installing package version {Version} ...", package, string.IsNullOrEmpty(version) ? "LATEST" : version);

        if (!(await _gitHubService.TryGetRelease(package, version))
            .Out(out var release))
        {
            return false;
        }

        if (release is null)
        {
            Log.Warning("No releases found for package {Package}", package);
            // clean slots for failed install
            _ = model.Slots.Remove(slotId);
            return false;
        }

        if (await _deploymentService.InstallReleaseAsync(package, release, slotId))
        {
            Log.Information("[{Package}] Package successfully installed", package);
        }
        else
        {
            // clean slots for failed install
            _ = model.Slots.Remove(slotId);
            return false;
        }

        // since we don't really have a concept of "global tools", we can pass global = false and the path
        return await InstallDependencies(package, model.Slots[slotId].FullPath.NotNullOrEmpty(), false);
    }

    private async Task<bool> InstallDependencies(Package package, string path, bool global)
    {
        var dependencies = package.Dependencies;
        if (dependencies is null)
        {
            return true;
        }

        var dependencyResult = true;
        if (dependencies.Count > 0)
        {
            Log.Information("[{Package}] Found {Count} dependencies. Installing...", package, dependencies.Count);
            foreach (var dep in dependencies)
            {
                dependencyResult = await Install(dep.Id, dep.Version, path, global);
            }
        }

        if (!dependencyResult)
        {
            Log.Warning("[{Package}] Some dependencies failed to install correctly", package);
            return false;
        }
        else
        {
            Log.Information("[{Package}] All dependencies installed successfully", package);
            return true;
        }


    }

    /// <summary>
    /// Gets the install path for a given global or local tool configuration
    /// </summary>
    /// <param name="path"></param>
    /// <param name="global"></param>
    /// <param name="package"></param>
    /// <param name="installPath"></param>
    /// <returns>false if invalid tool configuration</returns>
    public static bool TryGetInstallPath(Package package, string path, bool global, out string installPath, out bool isDefault)
    {
        // check if package is in local library
        // if not it just goes to slot 0
        isDefault = false;
        installPath = "";
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
            {
                installPath = IAppSettings.GetDefaultInstallDir(package);
                if (!Directory.Exists(installPath))
                {
                    Directory.CreateDirectory(installPath);
                }
                isDefault = true;
                break;
            }
            // not global & path        install in path
            case false when !isPathEmpty:
            {
                installPath = path;
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

        return true;
    }
}
