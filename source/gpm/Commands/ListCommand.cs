using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.Tasks;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class ListCommand : Command
    {
        private new const string Description = "Lists all installed packages.";
        private new const string Name = "list";

        public ListCommand() : base(Name, Description)
        {
            Handler = CommandHandler.Create<IHost>(ListAction.List);
        }
    }
}
