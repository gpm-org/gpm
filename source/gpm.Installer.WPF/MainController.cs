using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using gpm.Core.Services;
using Serilog;

namespace gpm.Installer.WPF;

internal class MainController
{
    private readonly ITaskService _taskService;
    private readonly ILibraryService _libraryService;

    public bool Restart { get; internal set; }
    public string RestartName { get; internal set; } = "";
    public string Package { get; internal set; } = "";

    public int Slot { get; internal set; }

    public MainController(ITaskService taskService, ILibraryService libraryService)
    {
        _taskService = taskService;
        _libraryService = libraryService;
    }

    public async Task<bool> RunAsync()
    {
        // checks
        if (!_libraryService.TryGetValue(Package, out var model))
        {
            Log.Warning("[{Package}] Package not found in library. Use gpm install to install a package", Package);
            return false;
        }
        if (!_libraryService.IsInstalled(Package))
        {
            Log.Warning("[{Package}] Package not installed. Use gpm install to install a package", Package);
            return false;
        }
        if (!_libraryService.IsInstalledInSlot(Package, Slot))
        {
            Log.Warning(
                "[{Package}] Package not installed in slot {SlotIdx}. Use gpm install to install a package",
                Package, Slot.ToString());
            return false;
        }

        // wait for process to finish
        if (!WaitForProcessEnded())
        {
            Log.Error("Could not kill process, aborting");
            return false;
        }

        // install in slot
        var result = await _taskService.UpgradeAndUpdate(Package, false, "", Slot, "");
        if (!result)
        {
            Log.Error("Could not update package");
            return false;
        }

        // restart app once finished
        if (Restart)
        {
            var baseDir = model.Slots[Slot].FullPath;
            if (baseDir == null)
            {
                Log.Error("No path registered for slot {Slot}", Slot);
                return false;
            }
            var exe = Path.Combine(baseDir, RestartName);

            if (string.IsNullOrEmpty(RestartName))
            {
                // use first exe in directory
                var files = Directory.GetFiles(baseDir, ".exe");
                exe = files.FirstOrDefault();

                if (exe == null)
                {
                    Log.Error("No app to restart in {BaseDir}, aborting", baseDir);
                    return false;
                }
            }

            if (!File.Exists(exe))
            {
                Log.Error("No app to restart in {BaseDir}, aborting", baseDir);
                return false;
            }

            Log.Information("Restarting ...");

            // restart app
            Process.Start(exe);
        }

        Log.Information("Shutting down ...");
        Application.Current.Shutdown();
        return true;
    }

    private bool WaitForProcessEnded()
    {
        Thread.Sleep(2000);

        var pname = Path.GetFileNameWithoutExtension(RestartName);
        var p = Process.GetProcessesByName(pname).FirstOrDefault();
        if (p == null)
        {
            Log.Information($"{pname} is null");
            Log.Information($"Waiting...");

            Thread.Sleep(2000);

            return true;
        }
        else
        {
            if (p.WaitForExit(5000))
            {
                Log.Information($"{pname} Has exited");
                return true;
            }
            else
            {
                Log.Error("Timeout");
                return false;
            }
        }
    }
}
