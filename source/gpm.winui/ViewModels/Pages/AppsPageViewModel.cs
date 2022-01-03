using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Nito.AsyncEx;
using gpm.core.Models;
using gpmWinui.Services;
using gpm.core.Services;

namespace gpmWinui.ViewModels.Pages;

public sealed class AppsPageViewModel : ObservableRecipient
{
    private readonly ILibraryService _libraryService = Ioc.Default.GetRequiredService<ILibraryService>();

    public AppsPageViewModel()
    {
        //_libraryService.PropertyChanged += LibraryService_PropertyChanged;

        Packages = new ObservableCollection<PackageModel>(_libraryService.Values);
    }

    private void LibraryService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            //case nameof(ILibraryService.Plugins):
            //    Plugins = new ObservableCollection<PluginViewModel>(_libraryService.Plugins.Values.Select(x => new PluginViewModel(x)));
            //    //await LoadPostsCommand.ExecuteAsync(null);
            //    break;
            default:
                break;
        }
    }


    public ObservableCollection<PackageModel> Packages { get; set; } = new();


    private PackageModel? _selectedPackage;
    public PackageModel? SelectedPackage
    {
        get => _selectedPackage;
        set => SetProperty(ref _selectedPackage, value);
    }


}

