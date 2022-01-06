using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using gpm.Core.Services;
using gpmWinui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gpmWinui.ViewModels
{
    public sealed class MainViewModel : ObservableObject
    {
        /// <summary>
        /// Gets the <see cref="IGitHubService"/> instance to use.
        /// </summary>
        private readonly ILibraryService _libraryService = Ioc.Default.GetRequiredService<ILibraryService>();

        /// <summary>
        /// Gets the <see cref="ISettingsService"/> instance to use.
        /// </summary>
        private readonly ISettingsService _settingsService = Ioc.Default.GetRequiredService<ISettingsService>();


        public MainViewModel()
        {
            _libraryService.Load();



            if (string.IsNullOrEmpty(_settingsService.Location))
            {
                _settingsService.Location = @"Z:\GOG Galaxy\Games\Cyberpunk 2077";
            }



        }


    }
}
