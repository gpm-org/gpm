namespace gpm.Core.Services;

public partial class TaskService
{
    public bool Upgrade() =>
        // TODO: check if git is installed

        _dataBaseService.FetchAndUpdateSelf();
}
