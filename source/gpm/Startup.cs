using gpm.Core;
using gpm.Core.Extensions;
using gpm.Core.Models;
using gpm.Core.Services;
using gpm.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace gpm;

public static class Startup
{
    public static void ConfigureAppConfiguration(IConfigurationBuilder configuration)
    {
        var appData = IAppSettings.GetAppDataFolder();
        var provider = new PhysicalFileProvider(appData);
        configuration.AddJsonFile(provider, Constants.APPSETTINGS, true, true);

        var baseFolder = Path.GetDirectoryName(AppContext.BaseDirectory);
        configuration.SetBasePath(baseFolder);
        configuration.AddJsonFile("appsettings.json");
    }

    public static void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddScoped<IAppSettings, AppSettings>();
        services.AddScoped<IProgressService<double>, PercentProgressService>();

        services.AddGpm();

        services.AddOptions<CommonSettings>()
            .Bind(ctx.Configuration.GetSection(nameof(CommonSettings)));
    }

    public static void ConfigureSerilog(HostBuilderContext ctx, IServiceProvider services, LoggerConfiguration logger)
    {
        var path = Path.Combine(IAppSettings.GetLogsFolder(), "gpm.txt");
        var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        logger
            .ReadFrom.Configuration(ctx.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: outputTemplate)
            .WriteTo.File(path, outputTemplate: outputTemplate, rollingInterval: RollingInterval.Day);
    }
}
