using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace gpm.cli.Commands
{
    public class ListCommand : Command
    {
        #region Fields

        private new const string Description = "";
        private new const string Name = "list";

        #endregion Fields

        #region Constructors

        public ListCommand() : base(Name, Description)
        {
            AddOption(new Option<string[]>(new[] { "--path", "-p" }, "Input archives path. Can be a file or a folder or a list of files/folders"));
            AddOption(new Option<string>(new[] { "--pattern", "-w" }, "Use optional search pattern (e.g. *.ink), if both regex and pattern is defined, pattern will be prioritized."));
            AddOption(new Option<string>(new[] { "--regex", "-r" }, "Use optional regex pattern."));
            AddOption(new Option<bool>(new[] { "--diff", "-d" }, "Dump archive json for diff"));
            AddOption(new Option<bool>(new[] { "--list", "-l" }, "List all files in archive"));

            Handler = CommandHandler.Create<string[], string, string, bool, bool, IHost>(Action);
        }

        private void Action(string[] path, string pattern, string regex, bool diff, bool list, IHost host)
        {
            var serviceProvider = host.Services;







        }

        #endregion Constructors
    }
}
