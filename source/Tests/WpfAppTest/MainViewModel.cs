using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using gpm.Core.Installer;
using Serilog;

namespace WpfAppTest;

public class MainViewModel : ObservableRecipient
{
    private readonly AutoInstallerService _installer = Ioc.Default.GetRequiredService<AutoInstallerService>();
    private readonly MySink _sink = Ioc.Default.GetRequiredService<MySink>();

    private readonly ReadOnlyObservableCollection<string> _list;

    public MainViewModel()
    {

        CheckCommand = new AsyncRelayCommand(CheckAsync, CanCheck);
        InstallCommand = new AsyncRelayCommand(InstallAsync);

        _text = "";

        var myOperation = _sink.Connect()
            .Transform(x => x.RenderMessage())
            .Bind(out _list)
            .DisposeMany()
            .Subscribe(x =>
            {
                foreach (var add in x)
                {
                    var msg = add.Item.Current;
                    Text += $"{msg}\n";
                }
            });

        CheckCommand.Execute(null);
    }

    private bool CanCheck() => _installer.IsEnabled;


    public IAsyncRelayCommand InstallCommand { get; }
    private async Task InstallAsync()
    {
        await Task.Delay(1);
    }

    public IAsyncRelayCommand CheckCommand { get; }
    private async Task CheckAsync()
    {
        var releases = await _installer.CheckForUpdate();

        var isUpdateAvailable = releases != null;
        Log.Information($"Is update available: {isUpdateAvailable}");

        if (releases != null)
        {
            // TODO: ask user

            // Option 1 sequential
            //await _installer.Update(releases);

            // Option 2 callback
            await _installer.Update(releases);
        }
    }

    private string _text;
    public string Text { get => _text; set => SetProperty(ref _text, value); }
}
