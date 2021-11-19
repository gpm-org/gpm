using gpm.cli.Services;
using gpm.core;
using gpm.core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace gpm.cli
{
    internal static class GenericHost
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    var appData = AppSettings.GetAppDataFolder();
                    var provider = new PhysicalFileProvider(appData);
                    configuration.AddJsonFile(provider, Constants.APPSETTINGS, true, true);

                    var baseFolder = Path.GetDirectoryName(System.AppContext.BaseDirectory);
                    configuration.SetBasePath(baseFolder);
                    configuration.AddJsonFile("appsettings.json");

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
                    services.AddScoped<IAppSettings, AppSettings>();
                    services.AddScoped<ILoggerService, MicrosoftLoggerService>();
                    services.AddScoped<IProgressService<double>, PercentProgressService>();

                    //services.AddSingleton<IHashService, HashService>();

                    services.AddOptions<CommonSettings>().Bind(hostContext.Configuration.GetSection(nameof(CommonSettings)));
                }
            );
    }
}
