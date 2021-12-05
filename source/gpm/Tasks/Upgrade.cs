using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Tasks
{
    public class Upgrade
    {
        public static void Action(IHost host)
        {
            var serviceProvider = host.Services;
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

            // TODO: check if git is installed

            dataBaseService.FetchAndUpdateSelf();
        }
    }
}
