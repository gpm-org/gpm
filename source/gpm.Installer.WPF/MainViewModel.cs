using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using gpm.Core.Services;
using Serilog;

namespace gpm.Installer.WPF;

public class MainViewModel : ObservableRecipient
{
    private readonly MySink _sink = Ioc.Default.GetRequiredService<MySink>();
    private readonly IProgressService<double> _progressService = Ioc.Default.GetRequiredService<IProgressService<double>>();
    private readonly MainController _mainController = Ioc.Default.GetRequiredService<MainController>();

    private readonly ReadOnlyObservableCollection<string> _list;

    public MainViewModel()
    {
        CloseCommand = new RelayCommand(Close, CanClose);
        CancelCommand = new RelayCommand(Cancel, CanCancel);

        IsNotFinished = true;
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

        _progressService.ProgressChanged += _progressService_ProgressChanged;
        _mainController.PropertyChanged += _mainController_PropertyChanged;

        //_ = Observable.FromEventPattern<EventHandler<double>, double>(
        //        handler => _progressService.ProgressChanged += handler,
        //        handler => _progressService.ProgressChanged -= handler)
        //        .Select(_ => _.EventArgs * 100)
        //        .ToProperty(this, x => x.Progress, out _progressValue);
    }

    private void _mainController_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "IsFinished":
                IsNotFinished = false;
                break;
            default:
                break;
        }


    }

    private bool _isNotFinished;
    public bool IsNotFinished { get => _isNotFinished; set => SetProperty(ref _isNotFinished, value); }

   

    private bool CanCancel() => !_mainController.IsFinished;
    private void Cancel() => Application.Current.Shutdown();

    private bool CanClose() => _mainController.IsFinished;
    private void Close() => Application.Current.Shutdown();

    public ICommand CloseCommand { get; }
    public ICommand CancelCommand { get; }

    private void _progressService_ProgressChanged(object? sender, double e) => ProgressValue = e * 100;

    private string _text;
    public string Text { get => _text; set => SetProperty(ref _text, value); }


    private double _progressValue;
    public double ProgressValue { get => _progressValue; set => SetProperty(ref _progressValue, value); }


}
