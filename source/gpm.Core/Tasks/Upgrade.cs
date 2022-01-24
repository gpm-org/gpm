namespace gpm.Core.Services;

public partial class TaskService
{
    public void Upgrade() =>
        // TODO: check if git is installed

        _dataBaseService.FetchAndUpdateSelf();
}
