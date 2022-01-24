using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using gpm.Core.Extensions;
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

                .AddScoped<IProgressService<double>, ProgressService<double>>()

                .AddGpm()

                .BuildServiceProvider());
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .WriteTo.Console()
              .WriteTo.MySink()
              .CreateLogger();

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
                    //case Constants.Commands.Dir:
                    //    mainController.BaseDir = commandValue;
                    //    break;
                    case Constants.Commands.Package:
                        mainController.Package = commandValue;
                        break;
                    case Constants.Commands.Slot:
                        if (int.TryParse(commandValue, out var slot))
                        {
                            mainController.Slot = slot;
                        }
                        break;
                }
            }

            //if (string.IsNullOrEmpty(mainController.BaseDir))
            //{
            //    mainController.BaseDir = AppContext.BaseDirectory;
            //    Log.Warning("No base directory set, using default: {BaseDir}", mainController.BaseDir);
            //}

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
