using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Nito.AsyncEx;
using RedCommunityToolkit.Services;
using gpm.core.Models;

namespace RedCommunityToolkit.ViewModels
{
    /// <summary>
    /// A viewmodel for a subreddit widget.
    /// </summary>
    public sealed class AppsPageViewModel : ObservableRecipient
    {
        
        /// <summary>
        /// Gets the <see cref="ISettingsService"/> instance to use.
        /// </summary>
        private readonly ILibraryService _libraryService = Ioc.Default.GetRequiredService<ILibraryService>();

        


        /// <summary>
        /// Creates a new <see cref="AppsPageViewModel"/> instance.
        /// </summary>
        public AppsPageViewModel()
        {
            _libraryService.PropertyChanged += LibraryService_PropertyChanged;

            Plugins = new ObservableCollection<PluginViewModel>(_libraryService.Plugins.Values.Select(x => new PluginViewModel(x)));
        }

        private void LibraryService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ILibraryService.Plugins):
                    Plugins = new ObservableCollection<PluginViewModel>(_libraryService.Plugins.Values.Select(x => new PluginViewModel(x)));
                    //await LoadPostsCommand.ExecuteAsync(null);
                    break;
                default:
                    break;
            }
        }

        

        /// <summary>
        /// Gets the collection of loaded posts.
        /// </summary>
        public ObservableCollection<PluginViewModel> Plugins { get; set; } = new();


        private PackageModel? _selectedPlugin;

        /// <summary>
        /// Gets or sets the currently selected subreddit.
        /// </summary>
        public PackageModel? SelectedPlugin
        {
            get => _selectedPlugin;
            set => SetProperty(ref _selectedPlugin, value);//_settingsService.SetValue(nameof(SelectedRepo), value);
        }

       
    }
}

