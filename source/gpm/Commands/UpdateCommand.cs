using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.Tasks;
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

            AddOption(new Option<bool>(new[] { "--all", "-a" },
                "Update all installed packages (only their default slots)."));
            AddOption(new Option<int>(new[] { "--slot", "-s" },
                "Update a specific slot. Input the index of the slot, default is 0."));

            AddOption(new Option<bool>(new[] { "--clean", "-c" },
                "Do a clean install and completely remove the installed package."));

            Handler = CommandHandler.Create<string, bool, int, bool, IHost>(Update.Action);
        }
    }
}
