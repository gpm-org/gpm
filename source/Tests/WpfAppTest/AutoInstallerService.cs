using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using gpm.Core;
using gpm.Core.Models;
using gpm.Core.Services;
using Octokit;
using Serilog;

namespace WpfAppTest;

/// <summary>
/// 
/// </summary>
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
        //Init();
    }

    /// <summary>
    /// Initializes the update manager
    /// </summary>
    /// <returns>false if no valid lockfile found in the app directory</returns>
    public bool Init()
    {
        // read manifest
        if (!TryGetLockFile(out var info))
        {
            return false;
        }

        var id = info.Packages[0].Id;
        var package = _dataBaseService.GetPackageFromName(id);
        if (package is null)
        {
            Log.Warning("Package {NameInner} not found in database", id);
            return false;
        }




        Package = package;
        IsEnabled = true;
        Version = info.Packages[0].Version;

        return true;
    }

    /// <summary>
    /// reads the apps directory for a lockfile and checks if an update is available
    /// 1 API call
    /// </summary>
    /// <returns>false if no valid lockfile found in the app directory or no update available</returns>
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
    /// reads the apps directory for a lockfile and checks if an update is available
    /// 1 API call
    /// </summary>
    /// <returns>false if no valid lockfile found in the app directory or no update available</returns>
    public async Task<IReadOnlyList<ReleaseModel>?> CheckForUpdateAndGetReleases()
    {
        if (Package is null || Version is null || !IsEnabled)
        {
            return null;
        }

        var releases = await _gitHubService.IsUpdateAvailableAndGetReleases(Package, Version);
        if (releases is not null)
        {
            Log.Warning("[{Package}] No update available for package", Package);
            return null;
        }

        return releases;
    }


    /// <summary>
    /// Updates the current app
    /// 1 API call
    /// </summary>
    /// <returns>true if update succeeded</returns>
    public async Task<bool> Update()   
    {
        var releases = await CheckForUpdateAndGetReleases();
        if (releases != null)
        {
            return false;
        }



             
        return true;
    }



    /// <summary>
    /// Ensure that the gpm global tool is installed
    /// and reinstalls it if not
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


    private static bool TryGetLockFile([NotNullWhen(true)] out PackageLock? packageLock)
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
