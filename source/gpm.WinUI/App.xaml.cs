using System;
using System.IO;
using CommunityToolkit.Mvvm.DependencyInjection;
using gpm.Core.Services;
using gpm.Core.Tasks;
using gpmWinui.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Serilog;
//using Refit;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace gpmWinui
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window m_window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public App()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                //.WriteTo.Console()
                .WriteTo.AppSink()
                .CreateLogger();


            // Register services
            Ioc.Default.ConfigureServices(
                new ServiceCollection()

                .AddSingleton<ISettingsService, SettingsService>()
                .AddScoped<IArchiveService, ArchiveService>()

                .AddSingleton<IGitHubService, GitHubService>()
                .AddSingleton<IDeploymentService, DeploymentService>()

                .AddSingleton<ILibraryService, LibraryService>()
                .AddSingleton<IDataBaseService, DataBaseService>()

                .AddSingleton<ITaskService, TaskService>()

                .BuildServiceProvider());

            m_window = new Shell();
            m_window.Activate();
        }

        //public static void ConfigureSerilog(HostBuilderContext ctx, IServiceProvider services, LoggerConfiguration logger)
        //{
        //    var path = Path.Combine(IAppSettings.GetLogsFolder(), "gpm.txt");
        //    var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        //    logger
        //        .ReadFrom.Configuration(ctx.Configuration)
        //        .Enrich.FromLogContext()
        //        .WriteTo.Console(outputTemplate: outputTemplate)
        //        .WriteTo.File(path, outputTemplate: outputTemplate, rollingInterval: RollingInterval.Day);
        //}
    }
}
