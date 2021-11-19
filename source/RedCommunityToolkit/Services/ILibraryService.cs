using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedCommunityToolkit.Models;

namespace RedCommunityToolkit.Services
{
    public interface ILibraryService : INotifyPropertyChanged, INotifyPropertyChanging
    {

        public Dictionary<string, PluginModel> Plugins { get; }

        Task LoadAsync();
    }
}
