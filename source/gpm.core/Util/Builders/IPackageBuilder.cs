using gpm.core.Models;

namespace gpm.core.Util.Builders
{
    public interface IPackageBuilder
    {
        /// <summary>
        /// Creates a default package builder
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateDefaultBuilder<T>(Package args) where T : IPackageBuilder, new()
        {
            T builder = new();
            return (T)builder.ConfigureDefaults(args);
        }

        public IPackageBuilder ConfigureDefaults(Package args);
    }

    public interface IPackageBuilder<in TIn, out TOut> : IPackageBuilder
    {
        TOut? Build(TIn releaseAssets);
    }
}
