using gpm.Core.Extensions;
using gpm.Core.Models;

namespace gpm.Core.Util.Builders;

public class AssetBuilder : IPackageBuilder<IEnumerable<ReleaseAssetModel>, ReleaseAssetModel>
{
    private BuilderContext? _builderContext;
    private Package? _package;

    private readonly List<Func<BuilderContext, IEnumerable<ReleaseAssetModel>, IEnumerable<ReleaseAssetModel>>>
        _configureIndexLogicActions = new();
    private readonly List<Func<BuilderContext, IEnumerable<ReleaseAssetModel>, IEnumerable<ReleaseAssetModel>>>
        _configureNamePatternLogicActions = new();


    public ReleaseAssetModel? Build(IEnumerable<ReleaseAssetModel> releaseAssets)
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


        void CreateHostBuilderContext() => _builderContext = new BuilderContext(_package);
    }

    public IPackageBuilder ConfigureDefaults(Package args)
    {
        _package = args;

        ConfigureIndexLogic((context, assets) => context.Package.AssetIndex is not { } i
        ? assets
        : new[] { assets.ToList()[i] });
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

            return assets.Where(x => matches.Contains(x.Name)).ToList();
        });

        return this;
    }

    private static string ReplacePlaceHolders(string name, Package package) =>
        name
            .Replace($"%{nameof(Package.Name)}%", package.Name)
            .Replace($"%{nameof(Package.Owner)}%", package.Owner)
            .Replace($"%{nameof(Package.Identifier)}%", package.Identifier);


    public IPackageBuilder ConfigureIndexLogic(
        Func<BuilderContext, IEnumerable<ReleaseAssetModel>, IEnumerable<ReleaseAssetModel>> configureDelegate)
    {
        _configureIndexLogicActions.Add(configureDelegate ??
                                        throw new ArgumentNullException(nameof(configureDelegate)));
        return this;
    }


    public IPackageBuilder ConfigureNamePatternLogic(
        Func<BuilderContext, IEnumerable<ReleaseAssetModel>, IEnumerable<ReleaseAssetModel>> configureDelegate)
    {
        _configureNamePatternLogicActions.Add(configureDelegate ??
                                        throw new ArgumentNullException(nameof(configureDelegate)));
        return this;
    }



}
