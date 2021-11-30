using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using gpm.core.Services;
using gpm.core.Util.Builders;
using gpm.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace gpm_tests
{
    [TestClass]
    public class CommandLineTests
    {
        private readonly IHost _host;

        private const string TESTNAME = "wolvenkit";
        private const string TESTNAME2 = @"https://github.com/Neurolinked/MlsetupBuilder.git";
        private const string TESTNAMEWRONG = "rfuzzo/hhhnotexisting";
        private const string TESTVERSION1 = "8.4.2";
        private const string TESTVERSION2 = "8.4.1";
        private const string TESTVERSIONWRONG = "xxxx";

        private static string GetTestSlot()
        {
            var folder = Path.Combine(IAppSettings.GetAppDataFolder(),
                "TESTSLOT"
            );
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        public CommandLineTests()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(IAppSettings.GetLogsFolder(), "gpm-tests-log.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _host = GenericHost.CreateHostBuilder(null).Build();

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            dataBaseService.FetchAndUpdateSelf();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            LogBeginOfTest();

            await Remove.Action(TESTNAME, 0, true, _host);
            await Remove.Action(TESTNAME2, 0, true, _host);

            Directory.Delete(GetTestSlot(),true);
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

            // test releasebuilder

            // test versionBuilder
            var release = releases[0];

            // test asset builder
            var assetBuilder = IPackageBuilder.CreateDefaultBuilder<AssetBuilder>(package);
            var asset = assetBuilder.Build(release.Assets);


        }

        [TestMethod]
        public async Task TestAll()
        {
            LogBeginOfTest();

            TestUpgrade();

            TestList();

            // cleanup
            await TestRemove();
            TestInstalled();

            await TestInstall();

            await TestUpdate();

            TestInstalled();

            await TestRemove();
            TestInstalled();
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

        [TestMethod]
        public void TestList()
        {
            LogBeginOfTest();

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

            dataBaseService.ListAllPackages();
        }

        [TestMethod]
        public void TestInstalled()
        {
            LogBeginOfTest();

            Installed.Action("", "", _host);
        }

        [TestMethod]
        public async Task TestInstall()
        {
            LogBeginOfTest();

            Log.Information("\n\n=> test installing a nonexisting package -> FAIL");
            Assert.IsFalse(await Install.Action(TESTNAMEWRONG, "", "", _host));

            Log.Information("\n\n=> test installing latest -> PASS");
            Assert.IsTrue(await Install.Action(TESTNAME, "", "", _host));
            Log.Information("\n\n=> test installing again -> FAIL");
            Assert.IsFalse(await Install.Action(TESTNAME, "", "", _host));
            Log.Information("\n\n=> test installing a wrong version -> FAIL");
            Assert.IsFalse(await Install.Action(TESTNAME, TESTVERSIONWRONG, "", _host));
            Log.Information("\n\n=> test installing a previous version into default slot  -> FAIL");
            Assert.IsFalse(await Install.Action(TESTNAME, TESTVERSION2, "", _host));

            Log.Information("\n\n=> test installing a previous version into new slot -> PASS");
            Assert.IsTrue(await Install.Action(TESTNAME, TESTVERSION1, GetTestSlot(), _host));
            Log.Information("\n\n=> test installing another version into new slot -> FAIL");
            Assert.IsFalse(await Install.Action(TESTNAME, TESTVERSION2, GetTestSlot(), _host));


            Log.Information("\n\n=> test installing another repo into default slot -> PASS");
            Assert.IsTrue(await Install.Action(TESTNAME2, "", "", _host));
            Log.Information("\n\n=> test installing another repo over an existing default slot -> FAIL");
            Assert.IsFalse(await Install.Action(TESTNAME2, "", GetTestSlot(), _host));

        }

        [TestMethod]
        public async Task TestUpdate()
        {   // test updating default
            await Update.Action(TESTNAME, false, 0, false, _host);

            // test updating default
            await Update.Action(TESTNAME, false, 0, false, _host);

            // test updating all
            await Update.Action(TESTNAME, false, 0, false, _host);


        }

        [TestMethod]
        public async Task TestRemove()
        {
            LogBeginOfTest();

            await Remove.Action(TESTNAME, 1, false, _host);

            await Remove.Action(TESTNAME, 0, true, _host);
        }


        private static void LogBeginOfTest([CallerMemberName] string methodName = "")
            => Console.WriteLine("\n=== {0} ===", methodName);
    }
}
