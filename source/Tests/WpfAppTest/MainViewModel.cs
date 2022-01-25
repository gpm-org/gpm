using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

        _installer
           .UseWPF()
           .AddLockFile()
           //.AddVersion("8.4.2")
           .AddChannel("Nightly", "wolvenkit/wolvenkit/test1")
           .AddChannel("Stable", "wolvenkit/wolvenkit/test1")
           .UseChannel("Stable")
           .Build();

        //CheckCommand.Execute(null);
    }

    private bool CanCheck() => _installer.IsEnabled;


    public IAsyncRelayCommand InstallCommand { get; }
    private async Task InstallAsync() => await Task.Delay(1);

    public IAsyncRelayCommand CheckCommand { get; }
    // 2 API calls
    private async Task CheckAsync()
    {
        Log.Information("CheckAsync");

        // 1 API call
        if (!(await _installer.CheckForUpdate())
            .Out(out var release))
        {
            return;
        }

        var isUpdateAvailable = release != null;
        Log.Information($"Is update available: {isUpdateAvailable}");

        if (release != null)
        {
            // TODO: ask user

            // Option 1 sequential
            //await _installer.Update(releases);

            // Option 2 callback
            // 1 API call
            await _installer.Update(release);
        }
    }

    private string _text;
    public string Text { get => _text; set => SetProperty(ref _text, value); }
}
