using System;
using System.Collections.Generic;
using gpm.core.Models;
using Microsoft.Extensions.DependencyInjection;
using Octokit;

namespace gpm.core.Util.Builders
{
    public static class AssetBuilderExtensions
    {
        // public static IPackageBuilder<T> ConfigureServices<T>(
        //     this IPackageBuilder<T> hostBuilder,
        //     Action<IServiceCollection> configureDelegate)
        // {
        //     return hostBuilder.ConfigureIndexLogic((context, collection) => configureDelegate(collection));
        // }

        // public static IPackageBuilder<T> ConfigureIndexLogic<T>(
        //     this IPackageBuilder<T> hostBuilder,
        //     Func<BuilderContext, IReadOnlyList<T>, IReadOnlyList<T>> configureDelegate)
        // {
        //     return hostBuilder.ConfigureIndexLogic((context, incollection) => configureDelegate(context, incollection));
        // }


        // public static IPackageBuilder ConfigureDefaults(this IPackageBuilder builder, Package args)
        // {
        //     builder.ConfigureDefaults(args);
        //
        //     return builder;
        // }


        // var idx = package.AssetIndex;
        // var asset = release.Assets[idx];
    }
}
