using gpm.Core.Exceptions;
using gpm.Core.Extensions;
using gpm.Core.Models;
using Serilog;

namespace gpm.Core.Tasks;

public partial class TaskService
{
    /// <summary>
    /// Update and updates a specified installed package
    /// </summary>
    /// <param name="name"></param>
    /// <param name="global"></param>
    /// <param name="path"></param>
    /// <param name="slot"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAndUpdate(string name, bool global, string path, int? slot, string version)
    {
        Upgrade();

        return await Update(name, global, path, slot, version);
    }


    /// <summary>
    /// Updates a specified installed package
    /// </summary>
    /// <param name="name"></param>
    /// <param name="global"></param>
    /// <param name="path"></param>
    /// <param name="slot"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public async Task<bool> Update(string name, bool global, string path, int? slot, string version)
    {
        #region checks

        if (string.IsNullOrEmpty(name))
        {
            Log.Warning("No package name specified. To update all installed packages use gpm update --all");
            return false;
        }

        var package = _dataBaseService.GetPackageFromName(name);
        if (package is null)
        {
            Log.Warning("Package {NameInner} not found in database", name);
            return false;
        }

        if (!_libraryService.TryGetValue(package.Id, out _))
        {
            Log.Warning("[{Package}] Package not found in library. Use gpm install to install a package", package);
            return false;
        }

        if (!_libraryService.IsInstalled(package))
        {
            Log.Warning("[{Package}] Package not installed. Use gpm install to install a package", package);
            return false;
        }

        if (global)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Log.Warning(
                    "[{Package}] --global specifies the installation is user wide. Can't be combined with the --path option",
                    package);
                return false;
            }

            if (slot is not null)
            {
                Log.Warning(
                    "[{Package}] --global specifies the installation is user wide. Can't be combined with the --slot option",
                    package);
                return false;
            }
        }

        if (slot is not null && !string.IsNullOrEmpty(path))
        {
            Log.Warning("[{Package}] please specify a --path option OR a --slot option, but not both", package);
            return false;
        }

        #endregion

        // try updating from --slot
        if (slot is not null)
        {
            if (!_libraryService.IsInstalledInSlot(package, slot.Value))
            {
                Log.Warning(
                    "[{Package}] Package not installed in slot {SlotIdx}. Use gpm install to install a package",
                    package, slot.ToString());
                return false;
            }

            return await UpdatePackageInSlot(package, slot.Value, version);
        }

        // get slot from path
        if (!TryGetInstallPath(package, path, global, out var installPath, out var isDefault))
        {
            return false;
        }
        if (!_libraryService.IsInstalledAtLocation(package, installPath, out var slotIdx))
        {
            Log.Warning("[{Package}] Package not installed in {Path} Use gpm install to install a package", package,
                installPath);
            return false;
        }

        return await UpdatePackageInSlot(package, slotIdx.Value, version);
    }

    private async Task<bool> UpdatePackageInSlot(Package package, int slotIdx, string version)
    {
        if (!_libraryService.TryGetValue(package.Id, out var model))
        {
            Log.Warning("[{Package}] Package not found in library. Use gpm install to install a package", package);
            return false;
        }

        var releases = await _gitHubService.GetReleasesForPackage(package);
        if (releases is null || !releases.Any())
        {
            Log.Warning("[{Package}] No releases found for package", package);
            return false;
        }
        if (!_gitHubService.IsUpdateAvailable(releases, model.Slots[slotIdx].Version.NotNull()))
        {
            Log.Warning("[{Package}] No update available for package", package);
            return false;
        }

        // uninstall package in location
        // save slot location for later re-install
        var installPath = model.Slots[slotIdx].FullPath;
        Log.Debug("[{Package}] Removing installed package ...", package);
        if (await _deploymentService.UninstallPackage(model.Key, slotIdx))
        {
            Log.Debug("[{Package}] Old package successfully removed", package);
        }
        else
        {
            Log.Warning("[{Package}] Failed to remove installed package. Aborting", package);
            return false;
        }

        // update to new version
        model.Slots.AddOrUpdate(slotIdx, new SlotManifest() { FullPath = installPath });
        Log.Information("[{Package}] Updating package ...", package);
        if (await _deploymentService.InstallReleaseAsync(package, releases, version, slotIdx))
        {
            Log.Information("[{Package}] Package successfully updated to version {Version}", package,
                model.Slots[slotIdx].Version);
            return true;
        }

        // handle dependencies
        // TODO

        Log.Warning("[{Package}] Failed to update package", package);
        return false;
    }
}
