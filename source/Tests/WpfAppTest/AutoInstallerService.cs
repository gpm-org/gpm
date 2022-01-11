using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using gpm.Core;
using gpm.Core.Models;
using gpm.Core.Services;
using Serilog;

namespace WpfAppTest;

public class AutoInstallerService
{
    private readonly IGitHubService _gitHubService = Ioc.Default.GetRequiredService<IGitHubService>();
    //private readonly ILibraryService _libraryService = Ioc.Default.GetRequiredService<ILibraryService>();
    private readonly IDataBaseService _dataBaseService = Ioc.Default.GetRequiredService<IDataBaseService>();

    public Package? Package { get; private set; }
    public bool IsEnabled { get; private set; }
    public string? Version { get; private set; }

    public AutoInstallerService()
    {

    }

    public async Task<bool> Init()
    {
        // read manifest
        var info = await TryGetLockFile();
        if (info is null)
        {
            return false;
        }
        var version = info.Packages[0].Version;
        if (version is null)
        {
            return false;
        }
        var id = info.Packages[0].Id;
        if (id is null)
        {
            return false;
        }

        var package = _dataBaseService.GetPackageFromName(id);
        if (package is null)
        {
            Log.Warning("Package {NameInner} not found in database", id);
            return false;
        }

        Package = package;
        IsEnabled = true;
        Version = version;

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CheckForUpdate()
    {
        if (Package is null || Version is null || !IsEnabled)
        {
            return false;
        }

        if (!await _gitHubService.IsUpdateAvailable(Package, Version))
        {
            Log.Warning("[{Package}] No update available for package", Package);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> EnsureGpmInstalled()
    {
        var installed = await InstallGpmAsync();
        if (!installed)
        {
            return await UpdateGpmAsync();
        }
        return true;
    }


    private static async Task<PackageLock?> TryGetLockFile()
    {
        var dir = AppContext.BaseDirectory;
        var lockFilePath = Path.Combine(dir, Constants.GPMLOCK);
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
                var obj = JsonSerializer.Deserialize<PackageLock>(await File.ReadAllTextAsync(lockFilePath), options);
                return obj;
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to read existing lock file");
            }
        }
        return null;
    }

    private static async Task<bool> UpdateGpmAsync() => await RunDotnetAsync("update", "gpm");

    private static async Task<bool> InstallGpmAsync() => await RunDotnetAsync("install", "gpm");

    private static async Task<bool> RunDotnetAsync(string verb, string toolName)
    {
        Process? p;
        TaskCompletionSource<bool>? _eventHandled;
        _eventHandled = new TaskCompletionSource<bool>();

        using (p = new Process())
        {
            try
            {
                p.StartInfo.FileName = "dotnet";
                p.StartInfo.Arguments = $"tool {verb} {toolName} -g";

                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                p.EnableRaisingEvents = true;

                p.OutputDataReceived += (s, e) =>
                {
                    Log.Information(e.Data);
                };
                p.ErrorDataReceived += (s, e) =>
                {
                    Log.Error(e.Data);
                };

                //p.Exited += new EventHandler(myProcess_Exited);
                p.Exited += (s, e) =>
                {
                    var exitCode = p.ExitCode;
                    Log.Information(
                        $"Exit time    : {p.ExitTime}\n" +
                        $"Exit code    : {exitCode}\n" +
                        $"Elapsed time : {Math.Round((p.ExitTime - p.StartTime).TotalMilliseconds)}");
                    _eventHandled.TrySetResult(true);
                };

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred trying to install gpm:\n{ex.Message}");
                return false;
            }

            // Wait for Exited event, but not more than 30 seconds.
            await await Task.WhenAny(_eventHandled.Task, Task.Delay(30000));
            return p.ExitCode == 0;
        }
    }
}
