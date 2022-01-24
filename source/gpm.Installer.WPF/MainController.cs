using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using gpm.Core.Services;
using Serilog;

namespace gpm.Installer.WPF;

internal class MainController : ObservableObject
{
    private readonly ITaskService _taskService;
    private readonly ILibraryService _libraryService;
    private readonly IProgressService<double> _progressService;

   

    public MainController(
        ITaskService taskService,
        ILibraryService libraryService,
        IProgressService<double> progressService
        )
    {
        _taskService = taskService;
        _libraryService = libraryService;
        _progressService = progressService;
    }

    private bool _isFinished;
    public bool IsFinished { get => _isFinished; set => SetProperty(ref _isFinished, value); }


    public bool Restart { get; internal set; }

    public string RestartName { get; internal set; } = "";

    public string Package { get; internal set; } = "";

    public int Slot { get; internal set; }


    public async Task<bool> RunAsync()
    {
        _progressService.Report(0);
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

        _progressService.Report(0.25);

        // wait for process to finish
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
            var files = Directory.GetFiles(baseDir, "*.exe");
            exe = files.FirstOrDefault();

            if (exe == null)
            {
                Log.Error("No app to restart in {BaseDir}, aborting", baseDir);
                return false;
            }
        }
        if (!WaitForProcessEnded(exe))
        {
            Log.Error("Could not kill process, aborting");
            return false;
        }

        // install in slot
        _progressService.Report(0.5);
        var result = await _taskService.UpgradeAndUpdate(Package, false, "", Slot, "");
        if (!result)
        {
            Log.Error("Could not update package");
            return false;
        }

        // restart app once finished
        _progressService.Report(0.75);
        if (Restart)
        {
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
        _progressService.Report(1);

        IsFinished = true;
        return true;
    }

    public async Task<bool> Test()
    {
        for (int i = 0; i < 3; i++)
        {
            _progressService.Report((i + 1) / 3);
            await Task.Delay(1000);
        }

        IsFinished = true;
        return true;
    }

    private bool WaitForProcessEnded(string exe)
    {
        if (string.IsNullOrEmpty(exe))
        {
            return true;
        }

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
