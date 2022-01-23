using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using DynamicData;

namespace gpm.Installer.WPF;

public class MainViewModel : ObservableRecipient
{
    private readonly MainController _mainController = Ioc.Default.GetRequiredService<MainController>();
    private readonly MySink _sink = Ioc.Default.GetRequiredService<MySink>();

    private readonly ReadOnlyObservableCollection<string> _list;

    public MainViewModel()
    {
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

        _mainController.Run();
    }

    private string _text;
    public string Text { get => _text; set => SetProperty(ref _text, value); }
}
