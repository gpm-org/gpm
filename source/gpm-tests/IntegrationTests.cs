using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using gpm.core.Services;
using static gpm_tests.Common;
using gpm.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace gpm_tests
{
    [TestClass]
    public class IntegrationTests
    {
        private readonly IHost _host;

        private const string TESTNAME = "wolvenkit/wolvenkit/test1";
        private const string TESTNAME2 = "wolvenkit/wolvenkit/test2";
        private const string TESTNAMEWRONG = "rfuzzo/hhhnotexisting";
        private const string TESTVERSION1 = "8.4.1";
        private const string TESTVERSION2 = "8.4.2";
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

        private static string GetTestSlot2()
        {
            var folder = Path.Combine(IAppSettings.GetAppDataFolder(),
                "TESTSLOT2"
            );
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        public IntegrationTests()
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

            Upgrade.Action(_host);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            LogBeginOfTest();

            await RemoveAction.Remove(TESTNAME, true, "", null, _host);
            await RemoveAction.Remove(TESTNAME, false, "", 0, _host);
            await RemoveAction.Remove(TESTNAME, false, "", 1, _host);

            await RemoveAction.Remove(TESTNAME2, true, "", null, _host);
            await RemoveAction.Remove(TESTNAME2, false, "", 0, _host);
            await RemoveAction.Remove(TESTNAME2, false, "", 1, _host);

            Directory.Delete(GetTestSlot(),true);
            Directory.Delete(GetTestSlot2(),true);
        }

        [TestMethod]
        public async Task TestList()
        {
            LogBeginOfTest();

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();

            await TestInstall();

            var installed = libraryService.GetInstalled();

            // TODO

        }

        [TestMethod]
        public async Task TestInstall()
        {
            LogBeginOfTest();

            Log.Information("\n\n=> test installing a nonexisting package -> FAIL");
            Assert.IsFalse(await InstallAction.Install(TESTNAMEWRONG, "", "", true, _host));

            Log.Information("\n\n=> test installing latest -> PASS");
            Assert.IsTrue(await InstallAction.Install(TESTNAME, "", "", true, _host));
            Log.Information("\n\n=> test installing again -> FAIL"); //TODO
            Assert.IsFalse(await InstallAction.Install(TESTNAME, "", "",true, _host));
            Log.Information("\n\n=> test installing a wrong version -> FAIL");
            Assert.IsFalse(await InstallAction.Install(TESTNAME, TESTVERSIONWRONG, "", true, _host));
            Log.Information("\n\n=> test installing a previous version into default slot  -> FAIL"); //TODO
            Assert.IsFalse(await InstallAction.Install(TESTNAME, TESTVERSION1, "",true, _host));

            Log.Information("\n\n=> test installing a version into new slot and global -> FAIL");
            Assert.IsFalse(await InstallAction.Install(TESTNAME, TESTVERSION1, GetTestSlot(), true, _host));

            Log.Information("\n\n=> test installing a previous version into new slot -> PASS");
            Assert.IsTrue(await InstallAction.Install(TESTNAME, TESTVERSION1, GetTestSlot(), false, _host));
            Log.Information("\n\n=> test installing another version into new slot -> FAIL");    //TODO
            Assert.IsFalse(await InstallAction.Install(TESTNAME, TESTVERSION2, GetTestSlot(), false, _host));


            Log.Information("\n\n=> test installing another repo into default slot -> PASS");
            Assert.IsTrue(await InstallAction.Install(TESTNAME2, "", "", true, _host));
            Log.Information("\n\n=> test installing another repo over an existing default slot -> PASS");
            Assert.IsTrue(await InstallAction.Install(TESTNAME2, "", GetTestSlot(), false, _host));

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

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();

            // try false input - fail by default
            Assert.IsFalse(await RemoveAction.Remove(TESTNAME, true, GetTestSlot(), 0, _host));
            Assert.IsFalse(await RemoveAction.Remove(TESTNAME, true, GetTestSlot(), null, _host));
            Assert.IsFalse(await RemoveAction.Remove(TESTNAME, true, "", 0, _host));

            // install a global tool in default: global && slot 0
            Assert.IsTrue(await InstallAction.Install(TESTNAME, TESTVERSION1, "", true, _host));
            // try removing from wrong slot - fail
            Assert.IsFalse(await RemoveAction.Remove(TESTNAME, false, "", 1, _host));
            Assert.IsFalse(await RemoveAction.Remove(TESTNAME, false, GetTestSlot(), null, _host));
            Assert.IsFalse(await RemoveAction.Remove(TESTNAME, false, GetTestSlot(), 0, _host));
            Assert.IsFalse(await RemoveAction.Remove(TESTNAME, false, GetTestSlot(), 1, _host));
            // try removing from current dir - fail
            Assert.IsFalse(await RemoveAction.Remove(TESTNAME, false, "", null, _host));
            // correct removal
            Assert.IsTrue(await RemoveAction.Remove(TESTNAME, true, "", null, _host));
            //Assert.IsTrue(await RemoveAction.Remove(TESTNAME, false, GetDefaultSlot(), null, _host));
            // would also work but are indeterminate
            //Assert.IsTrue(await RemoveAction.Remove(TESTNAME, false, "", 0, _host));

            // install a global tool in a custom slot
            Assert.IsTrue(await InstallAction.Install(TESTNAME, TESTVERSION1, GetTestSlot(), false, _host));
            // try global removal - fail
            Assert.IsFalse(await RemoveAction.Remove(TESTNAME, true, "", null, _host));
            // correct removal
            Assert.IsTrue(await RemoveAction.Remove(TESTNAME, false, GetTestSlot(), null, _host));
            // would also work but are indeterminate
            //Assert.IsTrue(await RemoveAction.Remove(TESTNAME, false, "", 0, _host));

            // install a local tool here
            Assert.IsTrue(await InstallAction.Install(TESTNAME, TESTVERSION1, "", false, _host));
            // try global removal - fail
            Assert.IsFalse(await RemoveAction.Remove(TESTNAME, true, "", null, _host));
            // correct removal
            Assert.IsTrue(await RemoveAction.Remove(TESTNAME, false, "", null, _host));
            //Assert.IsTrue(await RemoveAction.Remove(TESTNAME, false, GetCurrentDir(), null, _host));

            Assert.AreEqual(0, libraryService.GetInstalled().Count());
        }

    }
}
