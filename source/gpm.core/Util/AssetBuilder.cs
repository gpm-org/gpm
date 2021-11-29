using System;
using System.Collections.Generic;
using gpm.core.Models;
using Microsoft.Extensions.DependencyInjection;
using Octokit;

namespace gpm.core.Util
{
    public class AssetBuilderContext
    {

    }

    public interface IAssetBuilder
    {
        IAssetBuilder ConfigureServices(Action<AssetBuilderContext, IServiceCollection> configureDelegate);

        IAssetBuilder Build();

        ReleaseAsset GetAsset(IReadOnlyList<ReleaseAsset> releaseAssets);
    }

    public class AssetBuilder : IAssetBuilder
    {
        private bool _hostBuilt;
        private List<Action<AssetBuilderContext, IServiceCollection>> _configureServicesActions
            = new List<Action<AssetBuilderContext, IServiceCollection>>();

        public IAssetBuilder ConfigureServices(Action<AssetBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureServicesActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        /// <summary>
        /// Run the given actions to initialize the host. This can only be called once.
        /// </summary>
        /// <returns>An initialized <see cref="IAssetBuilder"/></returns>
        public IAssetBuilder Build()
        {
            if (_hostBuilt)
            {
                throw new InvalidOperationException("Build can only be called once.");
            }
            _hostBuilt = true;

            // BuildHostConfiguration();
            // CreateHostingEnvironment();
            // CreateHostBuilderContext();
            // BuildAppConfiguration();
            // CreateServiceProvider();

            return this;
        }

        public ReleaseAsset GetAsset(IReadOnlyList<ReleaseAsset> releaseAssets)
        {
            throw new NotImplementedException();
        }
    }

    public static class AssetHost
    {
        public static IAssetBuilder CreateDefaultBuilder(Package args)
        {
            AssetBuilder builder = new();
            return builder.ConfigureDefaults(args);
        }
    }

    public static class AssetBuilderExtensions
    {
        public static IAssetBuilder ConfigureServices(
            this IAssetBuilder hostBuilder,
            Action<IServiceCollection> configureDelegate)
        {
            return hostBuilder.ConfigureServices((context, collection) => configureDelegate(collection));
        }


        public static IAssetBuilder ConfigureDefaults(this IAssetBuilder builder, Package args)
        {
            //TODO

            return builder;
        }


        // var idx = package.AssetIndex;
        // var asset = release.Assets[idx];
    }
}
