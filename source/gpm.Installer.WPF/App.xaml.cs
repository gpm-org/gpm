using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using gpm.Core.Installer;
using gpm.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace gpm.Installer.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Register services
            Ioc.Default.ConfigureServices(
                new ServiceCollection()

                .AddSingleton<MySink>()

                .AddSingleton<MainController>()

                .AddSingleton<IGitHubService, GitHubService>()
                .AddSingleton<IDeploymentService, DeploymentService>()

                .AddSingleton<ILibraryService, LibraryService>()
                .AddSingleton<IDataBaseService, DataBaseService>()

                .BuildServiceProvider());
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console()
               .WriteTo.MySink()
               .CreateLogger();
            Log.Information("Started");
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            // Process command line args
            var mainController = Ioc.Default.GetRequiredService<MainController>();


            for (var i = 0; i != e.Args.Length; ++i)
            {
                var splits = e.Args[i].Split('=');
                var commandName = splits[0];
                var commandValue = splits[1];


                switch (commandName)
                {
                    case Constants.Commands.Restart:
                        mainController.Restart = true;
                        mainController.RestartName = commandValue;
                        break;
                    case Constants.Commands.Dir:
                        mainController.BaseDir = commandValue;
                        break;
                }
            }

            if (string.IsNullOrEmpty(mainController.BaseDir))
            {
                mainController.BaseDir = AppContext.BaseDirectory;
                Log.Warning("No base directory set, using default: {BaseDir}", mainController.BaseDir);
            }

            if (string.IsNullOrEmpty(mainController.RestartName))
            {
                // use first exe in directory
                var files = Directory.GetFiles(mainController.BaseDir, ".exe");
                var exeName = files.FirstOrDefault();

                if (exeName == null)
                {
                    Log.Error("No app to restart in {BaseDir}, aborting", mainController.BaseDir);
                    Application.Current.Shutdown();
                    return;
                }
            }

            // Create main application window, starting minimized if specified
            var mainWindow = new MainWindow();
            //if (restart)
            //{
            //    mainWindow.WindowState = WindowState.Minimized;
            //}
            mainWindow.Show();
        }
    }
}
