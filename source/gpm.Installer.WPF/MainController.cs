using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows;
using gpm.Core.Models;
using Serilog;

namespace gpm.Installer.WPF;

internal class MainController
{
    public bool Restart { get; internal set; }
    public string RestartName { get; internal set; } = "";

    public string BaseDir { get; internal set; } = "";

    public int Slot { get; internal set; }


    public void Run()
    {
        // checks
        // TODO


        // read lockfile
        //if (!TryGetLockFile(out var lockFile))
        //{
        //    Log.Error("No lockfile found in {BaseDir}", BaseDir);
        //    Application.Current.Shutdown();
        //    return;
        //}

        // wait for process to finish
        if (!WaitForProcessEnded())
        {
            Log.Error("Could not kill process, aborting");
            Application.Current.Shutdown();
            return;
        }

        // install in slot
        //TODO


        // restart app once finished
        if (Restart)
        {
            Log.Information("Restarting ...");

            var exeName = RestartName;
            Process.Start(exeName);
        }

        Log.Information("Shutting down ...");
        Application.Current.Shutdown();
    }

    private bool TryGetLockFile([NotNullWhen(true)] out PackageLock? packageLock)
    {
        var baseDirectory = BaseDir;

        Log.Information($"Using BaseDirectory: {baseDirectory}");

        var lockFilePath = Path.Combine(baseDirectory, gpm.Core.Constants.GPMLOCK);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        if (File.Exists(lockFilePath))
        {
            try
            {
                packageLock = JsonSerializer.Deserialize<PackageLock>(File.ReadAllText(lockFilePath), options);
                if (packageLock is null)
                {
                    return false;
                }
                var version = packageLock.Packages[0].Version;
                if (version is null)
                {
                    return false;
                }
                var id = packageLock.Packages[0].Id;
                if (id is null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to read existing lock file");
            }
        }
        packageLock = null;
        return false;
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
