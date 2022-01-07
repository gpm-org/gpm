namespace gpm.Core.Services;

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
