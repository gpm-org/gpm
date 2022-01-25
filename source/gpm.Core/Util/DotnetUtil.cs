namespace gpm.Core.Util;


public static class DotnetUtil
{
    /// <summary>
    /// Run dotnet.exe tool -g with specified arguments
    /// </summary>
    /// <param name="verb"></param>
    /// <param name="toolName"></param>
    /// <returns>true if exit code is 0</returns>
    public static async Task<bool> RunDotnetAsync(string verb, string toolName)
        => await ProcessUtil.RunProcessAsync("dotnet", new string[] { "tool", verb, toolName, "-g" });


}
