using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gpm.core.Models;

namespace gpmWinui.Services
{
    public interface ILibraryService : INotifyPropertyChanged, INotifyPropertyChanging
    {

        public Dictionary<string, PackageModel> Plugins { get; }

        Task LoadAsync();
    }
}
