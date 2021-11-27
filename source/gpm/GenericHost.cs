using System;
using System.IO;
using gpm.core;
using gpm.core.Models;
using gpm.core.Services;
using gpm.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace gpm
{
    internal static class GenericHost
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, configuration) =>
                {
                    var appData = IAppSettings.GetAppDataFolder();
                    var provider = new PhysicalFileProvider(appData);
                    configuration.AddJsonFile(provider, Constants.APPSETTINGS, true, true);

                    var baseFolder = Path.GetDirectoryName(AppContext.BaseDirectory);
                    configuration.SetBasePath(baseFolder);
                    configuration.AddJsonFile("appsettings.json");
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddColorConsoleLogger(configuration =>
                    {
                        configuration.LogLevels.Add(LogLevel.Debug, ConsoleColor.Green); //success
                        configuration.LogLevels.Add(LogLevel.Information, ConsoleColor.DarkYellow);
                        configuration.LogLevels.Add(LogLevel.Warning, ConsoleColor.DarkMagenta);
                        configuration.LogLevels.Add(LogLevel.Error, ConsoleColor.Red);
                        configuration.LogLevels.Add(LogLevel.Critical, ConsoleColor.DarkRed);
                    });
                })
                .ConfigureServices((hostContext, services) =>
                    {
                        services.AddScoped<IAppSettings, AppSettings>();
                        //services.AddScoped<ILoggerService, MicrosoftLoggerService>();
                        services.AddScoped<IProgressService<double>, PercentProgressService>();

                        services.AddSingleton<ILibraryService, LibraryService>();
                        services.AddSingleton<IDataBaseService, DataBaseService>();

                        services.AddSingleton<IDeploymentService, DeploymentService>();
                        services.AddSingleton<IGitHubService, GitHubService>();

                        services.AddOptions<CommonSettings>()
                            .Bind(hostContext.Configuration.GetSection(nameof(CommonSettings)));
                    }
                );
    }
}
