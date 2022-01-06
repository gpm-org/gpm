using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using gpm.Core.Services;
using gpm.Core.Util.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static gpm.Tests.Common;

namespace gpm.Tests
{
    [TestClass]
    public class UnitTests
    {
        private readonly IHost _host;

        private const string TESTNAME = "wolvenkit/wolvenkit/test1";
        private const string TESTVERSION1 = "8.4.2";

        public UnitTests()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(IAppSettings.GetLogsFolder(), "gpm-tests-log.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _host = GenericHost.CreateHostBuilder(null).Build();

            Environment.CurrentDirectory = Path.GetTempPath();

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            dataBaseService.FetchAndUpdateSelf();
        }

        [TestCleanup]
        public void Cleanup()
        {

        }



        [TestMethod]
        public async Task TestBuilders()
        {
            LogBeginOfTest();

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            var gitHubService = serviceProvider.GetRequiredService<IGitHubService>();

            var package = dataBaseService.GetPackageFromName(TESTNAME);
            ArgumentNullException.ThrowIfNull(package);

            var releases = await gitHubService.GetReleasesForPackage(package);
            ArgumentNullException.ThrowIfNull(releases);

            // TODO: test versionBuilder
            var release = releases.First(x => x.TagName.Equals(TESTVERSION1));

            // test asset builder
            var assetBuilder = IPackageBuilder.CreateDefaultBuilder<AssetBuilder>(package);
            var asset = assetBuilder.Build(release.Assets);

            Assert.IsNotNull(asset);
            Assert.AreEqual("manifest.json", asset.Name);

            // TODO: test install builder

        }

        [TestMethod]
        public void TestUpgrade()
        {
            LogBeginOfTest();

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

            dataBaseService.FetchAndUpdateSelf();
        }
    }
}
