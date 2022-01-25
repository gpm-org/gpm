namespace gpm.Core.Util;

public static class GpmUtil
{
    public enum ECommand
    {
        search,
        install,
        update,
        remove,
        list,
        restore,
        upgrade,
        run
    }


    /// <summary>
    /// Run gpm.exe
    /// </summary>
    /// <param name="command"></param>
    /// <param name="args"></param>
    /// <returns>true if exit code is 0</returns>
    public static async Task<bool> RunGpmAsync(ECommand command, params string[] args)
        => await ProcessUtil.RunProcessAsync("gpm", args.Concat( new string[] { command.ToString() }).ToArray());


    /// <summary>
    /// Ensure that the gpm global tool is installed
    /// and reinstalls it if not
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> EnsureGpmInstalled()
    {
        var installedOrUptodate = await UpdateGpmAsync();
        if (!installedOrUptodate)
        {
            return await InstallGpmAsync();
        }
        return true;
    }

    /// <summary>
    /// Update gpm as global dotnet tool
    /// </summary>
    /// <returns>true if latest installed, or if update succeeded. false otherwise</returns>
    public static async Task<bool> UpdateGpmAsync() => await DotnetUtil.RunDotnetAsync("update", "gpm");

    /// <summary>
    /// Install gpm as global dotnet tool
    /// </summary>
    /// <returns>false if already installed, true otherwise</returns>
    public static async Task<bool> InstallGpmAsync() => await DotnetUtil.RunDotnetAsync("install", "gpm");


}
