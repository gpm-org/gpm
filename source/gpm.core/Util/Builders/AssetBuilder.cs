using System;
using System.Collections.Generic;
using System.Linq;
using gpm.core.Extensions;
using gpm.core.Models;
using Octokit;

namespace gpm.core.Util.Builders
{
    public class AssetBuilder : IPackageBuilder<IReadOnlyList<ReleaseAsset>,ReleaseAsset>
    {
        private BuilderContext? _builderContext;
        private Package? _package;

        private readonly List<Func<BuilderContext, IReadOnlyList<ReleaseAsset>, IReadOnlyList<ReleaseAsset>>>
            _configureIndexLogicActions = new();
        private readonly List<Func<BuilderContext, IReadOnlyList<ReleaseAsset>, IReadOnlyList<ReleaseAsset>>>
            _configureNamePatternLogicActions = new();


        public ReleaseAsset? Build(IReadOnlyList<ReleaseAsset> releaseAssets)
        {
            ArgumentNullException.ThrowIfNull(_package);
            CreateHostBuilderContext();
            ArgumentNullException.ThrowIfNull(_builderContext);

            releaseAssets = _configureIndexLogicActions.Aggregate(releaseAssets,
                (current, configureServicesAction)
                    => configureServicesAction(_builderContext, current));
            releaseAssets = _configureNamePatternLogicActions.Aggregate(releaseAssets,
                (current, configureServicesAction)
                    => configureServicesAction(_builderContext, current));

            return releaseAssets.FirstOrDefault();


            void CreateHostBuilderContext()
            {
                _builderContext = new BuilderContext(_package);
            }
        }

        public IPackageBuilder ConfigureDefaults( Package args)
        {
            _package = args;

            ConfigureIndexLogic((context, assets) => context.Package.AssetIndex is not { } i ? assets : new[] { assets[i] });
            ConfigureNamePatternLogic((context, assets) =>
            {
                var pattern = context.Package.AssetNamePattern;
                // check search pattern then regex
                if (string.IsNullOrEmpty(pattern))
                {
                    return assets;
                }

                var matches = assets
                    .Select(x => x.Name)
                    .Select(x => ReplacePlaceHolders(x, context.Package))
                    .MatchesWildcard(x => x, pattern);

                return (IReadOnlyList<ReleaseAsset>)assets.Where(x => matches.Contains(x.Name));

            });

            return this;
        }

        private static string ReplacePlaceHolders(string name, Package package) =>
            name
                .Replace($"%{nameof(Package.Name)}%", package.Name)
                .Replace($"%{nameof(Package.Owner)}%", package.Owner)
                .Replace($"%{nameof(Package.Identifier)}%", package.Identifier);


        public IPackageBuilder ConfigureIndexLogic(
            Func<BuilderContext, IReadOnlyList<ReleaseAsset>, IReadOnlyList<ReleaseAsset>> configureDelegate)
        {
            _configureIndexLogicActions.Add(configureDelegate ??
                                            throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }


        public IPackageBuilder ConfigureNamePatternLogic(
            Func<BuilderContext, IReadOnlyList<ReleaseAsset>, IReadOnlyList<ReleaseAsset>> configureDelegate)
        {
            _configureNamePatternLogicActions.Add(configureDelegate ??
                                            throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }



    }
}
