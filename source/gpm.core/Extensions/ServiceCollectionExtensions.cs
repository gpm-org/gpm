using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gpm.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace gpm.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all gpm internal dependencies
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddGpm(this IServiceCollection services)
    {
        services.AddScoped<IArchiveService, ArchiveService>();
        services.AddSingleton<ILibraryService, LibraryService>();
        services.AddSingleton<IDataBaseService, DataBaseService>();
        services.AddSingleton<IDeploymentService, DeploymentService>();
        services.AddSingleton<IGitHubService, GitHubService>();
        services.AddSingleton<ITaskService, TaskService>();

        return services;
    }
}
