using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.Tasks;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class InstallCommand : Command
    {
        private new const string Description = "Install a package.";
        private new const string Name = "install";

        public InstallCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string>("name",
                "The package name. Can be a github repo url, a repo name or in the form of owner/name/id. "));

            AddOption(new Option<string>(new[] { "--version", "-v" },
                "A specific package version to install. Leave out or leave empty to install latest."));
            AddOption(new Option<string>(new[] { "--slot", "-s" },
                "A slot to install a new instance into. Must be a directory path."));

            Handler = CommandHandler.Create<string, string, string, IHost>(Install.Action);
        }
    }
}
