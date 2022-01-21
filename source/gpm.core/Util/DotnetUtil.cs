using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace gpm.Core.Util;

public static class DotnetUtil
{
    /// <summary>
    /// Run dotnet.exe tool with specified arguments
    /// </summary>
    /// <param name="verb"></param>
    /// <param name="toolName"></param>
    /// <returns></returns>
    public static async Task<bool> RunDotnetToolAsync(string verb, string toolName)
    {
        Process? p;
        var eventHandled = new TaskCompletionSource<bool>();

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
                    eventHandled.TrySetResult(true);
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
            await await Task.WhenAny(eventHandled.Task, Task.Delay(30000));
            return p.ExitCode == 0;
        }
    }
}
