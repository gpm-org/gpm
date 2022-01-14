using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Serilog;

namespace WpfAppTest;

public class MainViewModel : ObservableRecipient
{
    private readonly AutoInstallerService _installer = Ioc.Default.GetRequiredService<AutoInstallerService>();
    private readonly MySink _sink = Ioc.Default.GetRequiredService<MySink>();

    private readonly ReadOnlyObservableCollection<string> list;

    public MainViewModel()
    {

        CheckCommand = new AsyncRelayCommand(CheckAsync);
        InstallCommand = new AsyncRelayCommand(InstallAsync);

        text = "";

        InstallCommand.Execute(this);

        var myOperation = _sink.Connect()
            .Transform(x => x.RenderMessage())
            .Bind(out list)
            .DisposeMany()
            .Subscribe(x =>
            {
                foreach (var add in x)
                {
                    var msg = add.Item.Current;
                    Text += $"{msg}\n";
                }
            });
    }



    public IAsyncRelayCommand InstallCommand { get; }
    private async Task InstallAsync()
    {
        await AutoInstallerService.EnsureGpmInstalled();

        var autoUpdateEnabled = _installer.Init();
        Log.Information($"[{_installer.Package}, v.{_installer.Version}] auto-update Enabled: {autoUpdateEnabled}");
    }

    public IAsyncRelayCommand CheckCommand { get; }
    private async Task CheckAsync()
    {
        var isUpdateAvailable = false;
        var result = await _installer.CheckForUpdate();
        if (result != null)
        {
            isUpdateAvailable = true;
        }
        Log.Information($"Is update available: {isUpdateAvailable}");

    }

    private string text;
    public string Text { get => text; set => SetProperty(ref text, value); }
}
