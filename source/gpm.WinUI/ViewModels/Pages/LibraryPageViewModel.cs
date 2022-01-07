using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using gpm.Core.Services;

namespace gpm.WinUI.ViewModels.Pages;

/// <summary>
/// View model for installed packages
/// </summary>
public sealed class LibraryPageViewModel : ObservableRecipient
{
    private readonly ILibraryService _libraryService = Ioc.Default.GetRequiredService<ILibraryService>();

    public LibraryPageViewModel()
    {
        //_libraryService.PropertyChanged += LibraryService_PropertyChanged;

        Packages = new ObservableCollection<PackageModelViewModel>(_libraryService.Select(x => new PackageModelViewModel(x.Value)));
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

    public ObservableCollection<PackageModelViewModel> Packages { get; set; } = new();

    private PackageModelViewModel? _selectedPackage;
    public PackageModelViewModel? SelectedPackage
    {
        get => _selectedPackage;
        set => SetProperty(ref _selectedPackage, value);
    }
}

