using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using gpm.core.Models;

namespace gpm.core.Util.Builders
{
    public class InstallBuilder : IPackageBuilder<string,string>
    {
        private BuilderContext? _builderContext;
        private Package? _package;

        private readonly List<Func<BuilderContext, string, string>>
            _configureTagLogicActions = new();
        private readonly List<Func<BuilderContext, string, string>>
            _configurePathLogicActions = new();


        public string Build(string defaultDir)
        {
            ArgumentNullException.ThrowIfNull(_package);
            CreateHostBuilderContext();
            ArgumentNullException.ThrowIfNull(_builderContext);

            defaultDir = _configureTagLogicActions.Aggregate(defaultDir,
                (current, configureServicesAction)
                    => configureServicesAction(_builderContext, current));
            defaultDir = _configurePathLogicActions.Aggregate(defaultDir,
                (current, configureServicesAction)
                    => configureServicesAction(_builderContext, current));


            return defaultDir;


            void CreateHostBuilderContext()
            {
                _builderContext = new BuilderContext(_package);
            }
        }

        public IPackageBuilder ConfigureDefaults( Package args)
        {
            _package = args;

            ConfigureTagLogic((context, inPath) =>
            {
                var topics = context.Package.Topics;
                if (topics is null || topics.Length <= 0)
                {
                    return inPath;
                }

                List<string> topicList = new() { "cyberpunk2077", "mod" };
                if (topics.Select(x => x.ToLower()).Any(x => topicList.Contains(x)))
                {
                    throw new NotImplementedException();
                }

                return inPath;
            });
            ConfigurePathLogic((context, inPath) =>
            {
                var installPath = context.Package.InstallPath;
                if (string.IsNullOrEmpty(installPath))
                {
                    return inPath;
                }

                var replaced = ReplacePlaceHolders(inPath, context);
                return Directory.Exists(replaced)
                    ? replaced
                    : inPath;

            });

            return this;
        }

        private static string ReplacePlaceHolders(string name, BuilderContext context)
        {
            var interim = name
                .Replace($"%{nameof(Package.Name)}%", context.Package.Name)
                .Replace($"%{nameof(Package.Owner)}%", context.Package.Owner)
                .Replace($"%{nameof(Package.Identifier)}%", context.Package.Identifier);
            if (interim.Contains('%'))
            {

            }

            return interim;
        }


        public IPackageBuilder ConfigureTagLogic(
            Func<BuilderContext, string, string> configureDelegate)
        {
            _configureTagLogicActions.Add(configureDelegate ??
                                            throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }


        public IPackageBuilder ConfigurePathLogic(
            Func<BuilderContext, string, string> configureDelegate)
        {
            _configurePathLogicActions.Add(configureDelegate ??
                                            throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }



    }
}
