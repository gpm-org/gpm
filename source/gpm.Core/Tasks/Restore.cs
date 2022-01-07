using System.Text.Json;
using System.Text.Json.Serialization;
using gpm.Core.Models;
using Serilog;

namespace gpm.Core.Tasks;

public partial class TaskService
{
    /// <summary>
    /// Installs the local packages that are in scope for the current directory.
    /// </summary>
    /// <param name="host"></param>
    public async Task<bool> UpdateAndRestore()
    {
        //var serviceProvider = host.Services;
        //var library = serviceProvider.GetRequiredService<ILibraryService>();

        Upgrade();

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
            result = await Install(meta.Id, meta.Version, destinationDir, false);
        }

        if (result)
        {
            Log.Information("Restored {Count} packages", lockfile.Packages.Count);
        }

        return await Task.FromResult(result);
    }
}
