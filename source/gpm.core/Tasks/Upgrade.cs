using gpm.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Core.Tasks
{
    public partial class TaskService
    {
        public void Upgrade()
        {
            // TODO: check if git is installed

            _dataBaseService.FetchAndUpdateSelf();
        }
    }
}
