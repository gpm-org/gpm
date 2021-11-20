using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text.Json;
using gpm.core.Models;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.cli.Commands
{
    public class NewCommand : Command
    {
        #region Fields

        private new const string Description = "";
        private new const string Name = "new";

        #endregion Fields

        #region Constructors

        public NewCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string>("url", "github url."));

            AddOption(new Option<string?>(new[] { "--name", "-n" }, "repo name"));

            Handler = CommandHandler.Create<string, string?, IHost>(Action);
        }

        private void Action(string url, string? name, IHost host)
        {
            var serviceProvider = host.Services;
            var logger = serviceProvider.GetRequiredService<ILoggerService>();

            var fi = new FileInfo(url);
            if (fi.Directory is null)
            {
                throw new ArgumentException(nameof(url));
            }

            var rOwner = fi.Directory.Name;
            var rName = fi.Name.Split('.').First();

            if (string.IsNullOrEmpty(rOwner) || string.IsNullOrEmpty(rName))
            {
                throw new ArgumentException(nameof(url));
            }

            //if (string.IsNullOrEmpty(name))
            //{
            //    name = rName;
            //}

            //IReadOnlyList<Release>? releases;
            //try
            //{
            //    releases = await _client.Repository.Release.GetAll(rOwner, rName);
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
            //if (releases == null || !releases.Any())
            //{
            //    return false;
            //}

            var package = new Package(url)
            {
                Identifier = name
            };

            var currentDir = Path.GetFullPath(Environment.CurrentDirectory);
            var path = Path.Combine(currentDir, $"{rOwner}_{rName}_{name}.gpak");

            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(package, options);
            File.WriteAllText(path, jsonString);

            logger.Success($"created new package {path}");
        }

        #endregion Constructors
    }
}
