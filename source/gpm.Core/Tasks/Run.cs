using gpm.Core.Extensions;
using gpm.Core.Models;
using gpm.Core.Util;
using Serilog;

namespace gpm.Core.Services;

public partial class TaskService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public async Task<bool> UpgradeAndRun(string name, string[]? args)
    {
        Upgrade();

        return await Run(name, args);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public async Task<bool> Run(string name, string[]? args)
    {
        // checks
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

        if (!_libraryService.TryGetValue(package.Id, out var model))
        {
            Log.Warning("[{Package}] Package not found in library. Use gpm install to install a package", package);
            return false;
        }

        if (!_libraryService.IsInstalled(package))
        {
            Log.Warning("[{Package}] Package not installed. Use gpm install to install a package", package);
            return false;
        }

        // get slot from path
        if (!TryGetInstallPath(package, "", true, out var installPath, out var isDefault))
        {
            return false;
        }
        if (!_libraryService.IsInstalledAtLocation(package, installPath, out var slotIdx))
        {
            Log.Warning("[{Package}] Package not installed in {Path} Use gpm install to install a package", package,
                installPath);
            return false;
        }

        var baseDir = model.Slots[slotIdx.Value].FullPath;
        if (baseDir == null)
        {
            Log.Error("No path registered for slot {Slot}", slotIdx.Value);
            return false;
        }
        // TODO specify exe in package format
        //var exe = Path.Combine(baseDir, RestartName);
        var exe = "";
        //if (string.IsNullOrEmpty(RestartName))
        {
            // use first exe in directory
            // TODO cross platform
            var files = Directory.GetFiles(baseDir, "*.exe");
            exe = files.FirstOrDefault();

            if (exe == null)
            {
                Log.Error("No app to restart in {BaseDir}, aborting", baseDir);
                return false;
            }
        }

        var result = false;
        if (args is not null)
        {
            result = await ProcessUtil.RunProcessAsync(exe, args);
        }
        else
        {
            result = await ProcessUtil.RunProcessAsync(exe);
        }
        return result;
    }
}
