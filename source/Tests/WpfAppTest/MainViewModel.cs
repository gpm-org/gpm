using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DynamicData;

namespace WpfAppTest;

public class MainViewModel : ObservableRecipient
{
    private AutoInstallerService installer;

    private readonly MySink _sink = Ioc.Default.GetRequiredService<MySink>();

    private readonly ReadOnlyObservableCollection<string> list;

    public MainViewModel()
    {
        installer = new AutoInstallerService();

        CheckCommand = new AsyncRelayCommand(CheckAsync);
        InstallCommand = new AsyncRelayCommand(InstallAsync);

        //document = new FlowDocument();
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

                    //var paragraph = new Paragraph()
                    //{
                    //    LineHeight = 1
                    //};
                    //var run = new Run(msg)
                    //{
                    //    //Foreground = GetBrushForLevel(level)
                    //};
                    //paragraph.Inlines.Add(run);
                    //Document.Blocks.Add(paragraph);
                }
            });
    }



    public IAsyncRelayCommand InstallCommand { get; }
    private async Task InstallAsync()
    {
        await AutoInstallerService.EnsureGpmInstalled();
    }

    public IAsyncRelayCommand CheckCommand { get; }
    private async Task CheckAsync()
    {
        await AutoInstallerService.EnsureGpmInstalled();
    }

    //private FlowDocument document;

    //public FlowDocument Document { get => document; set => SetProperty(ref document, value); }

    private string text;
    public string Text { get => text; set => SetProperty(ref text, value); }
}
