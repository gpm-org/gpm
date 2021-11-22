using System;
using gpm.core.Models;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace gpm_util
{
    internal static class GenericHost
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {

                    // var baseFolder = Path.GetDirectoryName(AppContext.BaseDirectory);
                    // configuration.SetBasePath(baseFolder);
                    // configuration.AddJsonFile("appsettings.json");
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddColorConsoleLogger(configuration =>
                    {
                        configuration.LogLevels.Add(LogLevel.Warning, ConsoleColor.DarkYellow);
                        configuration.LogLevels.Add(LogLevel.Error, ConsoleColor.DarkMagenta);
                        configuration.LogLevels.Add(LogLevel.Critical, ConsoleColor.Red);
                    });
                })
                .ConfigureServices((hostContext, services) =>
                    {
                        //services.AddScoped<IAppSettings, AppSettings>();
                        services.AddScoped<ILoggerService, MicrosoftLoggerService>();
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