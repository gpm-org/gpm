namespace gpm.Core.Services;

public interface ITaskService
{
    void List();
    Task<bool> UpgradeAndRestore();

    Task<bool> UpgradeAndInstall(string name, string version, string path, bool global);
    Task<bool> Install(string name, string version, string path, bool global);

    Task<bool> UpgradeAndRemove(string name, bool global, string path, int? slot);
    Task<bool> Remove(string name, bool global, string path, int? slot);

    Task<bool> UpgradeAndUpdate(string name, bool global, string path, int? slot, string version);
    Task<bool> Update(string name, bool global, string path, int? slot, string version);

    void Upgrade();
    
}
