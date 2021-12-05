using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.Tasks;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{


    /// <summary>
    /// aliases: remove, rm, r, un, unlink


    /// </summary>
    public class RemoveCommand : Command
    {
        private new const string Description = "Uninstall a package";
        private new const string Name = "remove";

        public RemoveCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string>("name",
                "The package name. Can be a github repo url, a repo name or in the form of owner/name/id"));


            AddOption(new Option<string>(new[] { "--path", "-p" },
                "Specifies the location where to uninstall the package. PATH can be absolute or relative. Can't be combined with the --global option. Omitting both --global and --path specifies that the package to be removed is a local package."));
            AddOption(new Option<bool>(new[] { "--global", "-g" },
                "Specifies that the package to be removed is from a user-wide installation. Can't be combined with the --path option. Omitting both --global and --path specifies that the package to be removed is a local package."));
            AddOption(new Option<int?>(new[] { "--slot", "-s" },
                "The package slot to remove."));

            Handler = CommandHandler.Create<string, bool, string, int?, IHost>(RemoveAction.UpdateAndRemove);
        }
    }
}
