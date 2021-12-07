using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using gpm.core;
using gpm.core.Models;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace gpm.Tasks
{
    public static class RestoreAction
    {
        /// <summary>
        /// Installs the local packages that are in scope for the current directory.
        /// </summary>
        /// <param name="host"></param>
        public static async Task<bool> RestoreAndUpdate(IHost host)
        {
            //var serviceProvider = host.Services;
            //var library = serviceProvider.GetRequiredService<ILibraryService>();

            Upgrade.Action(host);

            var destinationDir = Environment.CurrentDirectory;
            var lockFilePath = Path.Combine(destinationDir, Constants.GPMLOCK);
            if (!File.Exists(lockFilePath))
            {
                Log.Warning("No gpm-lock.json file found in current directory. Nothing to restore");
                return await Task.FromResult(false);
            }

            PackageLock lockfile = new();
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var obj = JsonSerializer.Deserialize<PackageLock>(await File.ReadAllTextAsync(lockFilePath), options);
                if (obj is not null)
                {
                    lockfile = obj;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to read existing lock file");
            }

            var result = true;
            foreach (var meta in lockfile.Packages)
            {
                result = await InstallAction.Install(meta.Id, meta.Version, destinationDir, false, host);
            }

            if (result)
            {
                Log.Information("Restored {Count} packages", lockfile.Packages.Count);
            }

            return await Task.FromResult(result);
        }
    }
}
