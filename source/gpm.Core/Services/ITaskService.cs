using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace gpm.core.Services
{
    public interface ITaskService
    {
        void List();
        Task<bool> UpdateAndRestore();

        Task<bool> UpdateAndInstall(string name, string version, string path, bool global);
        Task<bool> Install(string name, string version, string path, bool global);

        Task<bool> UpdateAndRemove(string name, bool global, string path, int? slot);
        Task<bool> Remove(string name, bool global, string path, int? slot);

        Task<bool> Update(string name, bool global, string path, int? slot, string version);

        void Upgrade();
    }
}
