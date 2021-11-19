using gpm.cli.Services;
using gpm.core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                    var assemblyFolder = Path.GetDirectoryName(System.AppContext.BaseDirectory);

                    configuration.SetBasePath(assemblyFolder);
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
                    services.AddScoped<ILoggerService, MicrosoftLoggerService>();
                    services.AddScoped<IProgressService<double>, PercentProgressService>();

                    //services.AddSingleton<IHashService, HashService>();


                }
            );
    }
}
