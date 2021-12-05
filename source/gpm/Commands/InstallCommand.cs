using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.Tasks;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    /// <summary>
    /// Examples:
    /// gpm install redscript       installs redscript in the current directory
    /// gpm install redscript -g    installs redscript in the default location
    /// gpm install -p PATH         installs redscript in the specified location
    /// gpm install -g -p PATH      not allowed
    /// </summary>
    public class InstallCommand : Command
    {
        private new const string Description = "Install a package.";
        private new const string Name = "install";

        public InstallCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string>("name",
                "The package name. Can be a github repo url, a repo name or in the form of owner/name/id. "));

            AddOption(new Option<string>(new[] { "--version", "-v" },
                "The version of the package to install. By default, the latest stable package version is installed. Use this option to install preview or older versions of the package."));
            AddOption(new Option<string>(new[] { "--path", "-p" },
                "Specifies the location where to install the global package. PATH can be absolute or relative. If PATH doesn't exist, the command tries to create it. Omitting both --global and --path specifies a local package installation."));
            AddOption(new Option<bool>(new[] { "--global", "-g" },
                "Specifies that the installation is user wide. Can't be combined with the --path option. Omitting both --global and --tool-path specifies a local tool installation."));

            Handler = CommandHandler.Create<string, string, string, bool, IHost>(InstallAction.UpdateAndInstall);
        }
    }
}
