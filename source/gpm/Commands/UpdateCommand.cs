using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.core.Services;
using gpm.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class UpdateCommand : Command
    {
        private new const string Description = "Update an installed package.";
        private new const string Name = "update";

        public UpdateCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string>("name",
                "The package name. Can be a github repo url, a repo name or in the form of owner/name/id"));


            AddOption(new Option<bool>(new[] { "--global", "-g" },
                "Specifies that the update is for a user-wide package. Can't be combined with the `--path` option. Omitting both `--global` and `--path` specifies that the package to be updated is a local package."));
            AddOption(new Option<string>(new[] { "--path", "-p" },
                "Specifies the location where the global package is installed. PATH can be absolute or relative. Can't be combined with the `--global` option. Omitting both `--global` and `--path` specifies that the package to be updated is a local package."));
            AddOption(new Option<int?>(new[] { "--slot", "-s" },
                "Update a specific slot. Input the index of the slot, default is 0."));
            AddOption(new Option<string>(new[] { "--version", "-v" },
                "The version range of the tool package to update to. This cannot be used to downgrade versions, you must `uninstall` newer versions first."));

            Handler = CommandHandler.Create<string, bool, string, int?, string, IHost>(Update);
        }

        private void Update(string name, bool global, string path, int? slot, string version, IHost host)
        {
            var serviceProvider = host.Services;
            var taskService = serviceProvider.GetRequiredService<ITaskService>();

            taskService.Update(name, global, path, slot, version);
        }
    }
}
