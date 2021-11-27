using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.Tasks;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class RemoveCommand : Command
    {
        private new const string Description = "";
        private new const string Name = "remove";

        public RemoveCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string>("name",
                "The package name. Can be a github repo url, a repo name or in the form of owner/name/id"));

            AddOption(new Option<int>(new[] { "--slot", "-s" },
                "The package slot to remove."));
            AddOption(new Option<bool>(new[] { "--all", "-a" },
                "Remove package from all installed slots."));

            Handler = CommandHandler.Create<string, int, bool, IHost>(Remove.Action);
        }
    }
}
