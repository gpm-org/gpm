using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using gpm.core.Models;
using gpm.core.Services;
using ProtoBuf;

namespace gpm.core.Util
{
    public static class DatabaseUtil
    {
        public static void UpdateDatabase(ILoggerService logger)
        {
            var files = Directory.GetFiles(IAppSettings.GetGitDbFolder(), "*.gpak", SearchOption.AllDirectories);
            var packages = new List<Package>();
            foreach (var file in files)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                Package? package;
                try
                {
                    package = JsonSerializer.Deserialize<Package>(File.ReadAllText(file));
                }
                catch (Exception e)
                {
                    package = null;
                    logger.Error(e);
                }

                if (package is not null)
                {
                    packages.Add(package);
                }
            }

            try
            {
                using var file = File.Create(IAppSettings.GetDbFile());
                Serializer.Serialize(file, packages);
            }
            catch (Exception)
            {
                if (File.Exists(IAppSettings.GetDbFile()))
                {
                    File.Delete(IAppSettings.GetDbFile());
                }
                throw;
            }

            logger.Success("Database updated");
        }
    }
}
