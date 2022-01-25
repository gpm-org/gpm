namespace gpm.Core.Services;

public partial class TaskService : ITaskService
{
    protected readonly IDataBaseService _dataBaseService;
    protected readonly ILibraryService _libraryService;
    protected readonly IGitHubService _gitHubService;
    protected readonly IDeploymentService _deploymentService;

    public TaskService(
        IDataBaseService dataBaseService,
        ILibraryService libraryService,
        IGitHubService gitHubService,
        IDeploymentService deploymentService
        )
    {
        _dataBaseService = dataBaseService;
        _libraryService = libraryService;
        _gitHubService = gitHubService;
        _deploymentService = deploymentService;
    }


}
