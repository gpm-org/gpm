using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Nito.AsyncEx;
using RedCommunityToolkit.Models;
using RedCommunityToolkit.Services;

namespace RedCommunityToolkit.ViewModels
{
    /// <summary>
    /// A viewmodel for a subreddit widget.
    /// </summary>
    public sealed class AppsPageViewModel : ObservableRecipient
    {
        /// <summary>
        /// Gets the <see cref="IGitHubService"/> instance to use.
        /// </summary>
        private readonly IGitHubService _gitHubService = Ioc.Default.GetRequiredService<IGitHubService>();

        /// <summary>
        /// Gets the <see cref="ISettingsService"/> instance to use.
        /// </summary>
        private readonly ILibraryService _libraryService = Ioc.Default.GetRequiredService<ILibraryService>();

        /// <summary>
        /// An <see cref="AsyncLock"/> instance to avoid concurrent requests.
        /// </summary>
        private readonly AsyncLock _loadingLock = new();


        /// <summary>
        /// Creates a new <see cref="AppsPageViewModel"/> instance.
        /// </summary>
        public AppsPageViewModel()
        {
            LoadPostsCommand = new AsyncRelayCommand(LoadPostsAsync);

            _libraryService.PropertyChanged += LibraryService_PropertyChanged;

            Plugins = new ObservableCollection<PluginModel>(_libraryService.Plugins.Values.ToList());
        }

        private void LibraryService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ILibraryService.Plugins):
                    Plugins = new ObservableCollection<PluginModel>(_libraryService.Plugins.Values.ToList());
                    //await LoadPostsCommand.ExecuteAsync(null);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for loading posts.
        /// </summary>
        public IAsyncRelayCommand LoadPostsCommand { get; }

        /// <summary>
        /// Gets the collection of loaded posts.
        /// </summary>
        public ObservableCollection<PluginModel> Plugins { get; set; } = new();


        private PluginModel? _selectedPlugin;

        /// <summary>
        /// Gets or sets the currently selected subreddit.
        /// </summary>
        public PluginModel? SelectedPlugin
        {
            get => _selectedPlugin;
            set => SetProperty(ref _selectedPlugin, value);//_settingsService.SetValue(nameof(SelectedRepo), value);
        }

        /// <summary>
        /// Loads the posts from a specified subreddit.
        /// </summary>
        private async Task LoadPostsAsync()
        {
            using (await _loadingLock.LockAsync())
            {
                try
                {
                    foreach (var item in Plugins)
                    {

                        var response = await _gitHubService.GetGitHubRepoAsync(item);
                    }

                }
                catch
                {
                    // Whoops!
                }
            }



        }
    }
}

