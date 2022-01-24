using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using gpm.Core.Extensions;
using gpm.Core.Installer;
using gpm.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace WpfAppTest;

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
            .AddSingleton<AutoInstallerService>()

            .AddGpm()

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

        var _installer = Ioc.Default.GetRequiredService<AutoInstallerService>();

        _installer
            .UseWPF()
            .AddLockFile()
            //.AddVersion("8.4.2")
            .AddChannel("Nightly", "wolvenkit/wolvenkit/test1")
            .AddChannel("Stable", "wolvenkit/wolvenkit/test1")
            .UseChannel("Stable")
            .Build();

        Log.Information("Started");
    }



}
