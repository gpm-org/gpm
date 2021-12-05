using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.Tasks;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class RestoreCommand : Command
    {
        private new const string Description = "Restore packages defined in the local package lock file.";
        private new const string Name = "restore";

        public RestoreCommand() : base(Name, Description)
        {
            Handler = CommandHandler.Create<IHost>(RestoreAction.RestoreAndUpdate);
        }
    }
}
