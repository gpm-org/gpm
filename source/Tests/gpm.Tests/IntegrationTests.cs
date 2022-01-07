using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using gpm.core.Services;
using static gpm_tests.Common;
using gpm.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gpm.core;

namespace gpm_tests
{
    [TestClass]
    public class IntegrationTests
    {
        private readonly IHost _host;

        private const string TESTNAME1 = "wolvenkit/wolvenkit/test1";
        private const string TESTNAME2 = "jac3km4/redscript";
        private const string TESTNAME3 = "wolvenkit/wolvenkit/test2"; // dep: TESTNAME2
        private const string TESTNAMEWRONG = "rfuzzo/hhhnotexisting";
        private const string TESTVERSION1 = "8.4.1";
        private const string TESTVERSION2 = "8.4.2";
        private const string TESTVERSIONWRONG = "xxxx";
        private readonly string TESTSLOT0 = GetTestSlot(0);
        private readonly string TESTSLOT1 = GetTestSlot(1);
        private readonly string TESTSLOT2 = GetTestSlot(2);
        private readonly string TESTSLOT3 = GetTestSlot(3);

        private ITaskService taskService;

        private static string GetTestSlot(int i)
        {
            //Path.Combine(
           

            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Constants.APPDATA,
                $"TESTSLOT{i}"
            );
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        private static string GetTestFile(int i) =>
            i switch
            {
                1 => "manifest.json",
                2 => "redscript-cli.exe",
                3 => "manifest.json",
                _ => throw new ArgumentException()
            };

        private static string GetFileInSlot(int slot, int fileId) => Path.Combine(GetTestSlot(slot), GetTestFile(fileId));

        public IntegrationTests()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(IAppSettings.GetLogsFolder(), "gpm-tests-log.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _host = GenericHost.CreateHostBuilder(null).Build();

            Environment.CurrentDirectory = TESTSLOT0;

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            taskService = serviceProvider.GetRequiredService<ITaskService>();

            taskService.Upgrade();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            LogBeginOfTest();

            await taskService.Remove(TESTNAME1, true, "", null);
            await taskService.Remove(TESTNAME2, true, "", null);
            await taskService.Remove(TESTNAME3, true, "", null);
            for (var i = 0; i < 4; i++)
            {
                await taskService.Remove(TESTNAME1, false, "", i);
            }
            for (var i = 0; i < 4; i++)
            {
                await taskService.Remove(TESTNAME2, false, "", i);
            }
            for (var i = 0; i < 4; i++)
            {
                await taskService.Remove(TESTNAME3, false, "", i);
            }

            //Directory.Delete(TESTSLOT0, true);
            Directory.Delete(TESTSLOT1, true);
            Directory.Delete(TESTSLOT2, true);
            Directory.Delete(TESTSLOT3, true);
        }

        [TestMethod]
        public async Task TestList()
        {
            LogBeginOfTest();

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();

            taskService.List();

            Assert.IsTrue(await taskService.Install(TESTNAME1, "", "", true));
            var installed = libraryService.GetInstalled();
            Assert.IsTrue(installed.Any(x => x.Key.Equals(TESTNAME1)));
        }

        [TestMethod]
        public async Task TestInstall()
        {
            LogBeginOfTest();

            // default fails
            //test installing a nonexisting package -> FAIL");
            Assert.IsFalse(await taskService.Install(TESTNAMEWRONG, "", "", true));
            //test installing a version into new slot and global -> FAIL");
            Assert.IsFalse(await taskService.Install(TESTNAME1, TESTVERSION1, TESTSLOT1, true));
            //test installing a wrong version -> FAIL");
            Assert.IsFalse(await taskService.Install(TESTNAME1, TESTVERSIONWRONG, "", true));

            // global tool
            //test installing latest -> PASS");
            Assert.IsTrue(await taskService.Install(TESTNAME1, "", "", true));
            //TODO get default file
            //test installing again -> FAIL"); //TODO
            Assert.IsFalse(await taskService.Install(TESTNAME1, "", "",true));
            //test installing a previous version  -> FAIL"); //TODO
            Assert.IsFalse(await taskService.Install(TESTNAME1, TESTVERSION1, "",true));

            // global tool in new slot
            //test installing a previous version into new slot -> PASS");
            Assert.IsTrue(await taskService.Install(TESTNAME1, TESTVERSION1, TESTSLOT1, false));
            Assert.IsTrue(File.Exists(GetFileInSlot(1, 1)));
            //test installing another version into new slot -> FAIL");    //TODO
            Assert.IsFalse(await taskService.Install(TESTNAME1, TESTVERSION2, TESTSLOT1, false));

            // new global tool in default
            //test installing another repo into default slot -> PASS");
            Assert.IsTrue(await taskService.Install(TESTNAME2, "", "", true));
            //test installing another repo over an existing slot -> PASS");
            Assert.IsTrue(await taskService.Install(TESTNAME2, "", TESTSLOT1, false));
            Assert.IsTrue(File.Exists(GetFileInSlot(1, 2)));

            // install local tools
            Assert.IsTrue(await taskService.Install(TESTNAME1, "", "", false));
            Assert.IsTrue(File.Exists(GetFileInSlot(0, 1)));
            Assert.IsTrue(await taskService.Install(TESTNAME2, "", "", false));
            Assert.IsTrue(File.Exists(GetFileInSlot(0, 1)));

            // install deps
            Assert.IsTrue(await taskService.Install(TESTNAME3, "", TESTSLOT3, false));
            Assert.IsTrue(File.Exists(GetFileInSlot(3, 3)));
            Assert.IsTrue(File.Exists(GetFileInSlot(3, 2)));
        }

        [TestMethod]
        public async Task TestUpdate()
        {
            // test update global tool
            Assert.IsTrue(await taskService.Install(TESTNAME1, TESTVERSION1, "", true));
            // default fails
            Assert.IsFalse(await taskService.Update(TESTNAME1, true, TESTSLOT1, null, TESTVERSION1));
            Assert.IsFalse(await taskService.Update(TESTNAME1, true, "", 0, TESTVERSION1));
            Assert.IsFalse(await taskService.Update(TESTNAME1, true, TESTSLOT1, 0, TESTVERSION1));
            Assert.IsFalse(await taskService.Update(TESTNAME1, false, TESTSLOT1, 0, TESTVERSION1));

            Assert.IsTrue(await taskService.Update(TESTNAME1, true, "", null, TESTVERSION2));
            Assert.IsTrue(await taskService.Update(TESTNAME1, true, "", null, ""));

            // test updating global custom tool
            Assert.IsTrue(await taskService.Install(TESTNAME1, TESTVERSION1, TESTSLOT1, false));
            Assert.IsTrue(await taskService.Update(TESTNAME1, false, TESTSLOT1, null, TESTVERSION2));

            // test updating local tool
            Assert.IsTrue(await taskService.Install(TESTNAME1, TESTVERSION1, "", false));
            Assert.IsTrue(await taskService.Update(TESTNAME1, false, "", null, TESTVERSION2));

            // TODO: test dependency updating

        }

        [TestMethod]
        public async Task TestRemove()
        {
            LogBeginOfTest();

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();

            // try false input - fail by default
            Assert.IsFalse(await taskService.Remove(TESTNAME1, true, TESTSLOT1, 0));
            Assert.IsFalse(await taskService.Remove(TESTNAME1, true, TESTSLOT1, null));
            Assert.IsFalse(await taskService.Remove(TESTNAME1, true, "", 0));

            // install a global tool in default: global && slot 0
            Assert.IsTrue(await taskService.Install(TESTNAME1, TESTVERSION1, "", true));
            // try removing from wrong slot - fail
            Assert.IsFalse(await taskService.Remove(TESTNAME1, false, "", 1));
            Assert.IsFalse(await taskService.Remove(TESTNAME1, false, TESTSLOT1, null));
            Assert.IsFalse(await taskService.Remove(TESTNAME1, false, TESTSLOT1, 0));
            Assert.IsFalse(await taskService.Remove(TESTNAME1, false, TESTSLOT1, 1));
            // try removing from current dir - fail
            Assert.IsFalse(await taskService.Remove(TESTNAME1, false, "", null));
            // correct removal
            Assert.IsTrue(await taskService.Remove(TESTNAME1, true, "", null));
            //Assert.IsTrue(await taskService.Remove(TESTNAME, false, GetDefaultSlot(), null));
            // would also work but are indeterminate
            //Assert.IsTrue(await taskService.Remove(TESTNAME, false, "", 0));

            // install a global tool in a custom slot
            Assert.IsTrue(await taskService.Install(TESTNAME1, TESTVERSION1, TESTSLOT1, false));
            // try global removal - fail
            Assert.IsFalse(await taskService.Remove(TESTNAME1, true, "", null));
            // correct removal
            Assert.IsTrue(await taskService.Remove(TESTNAME1, false, TESTSLOT1, null));
            // would also work but are indeterminate
            //Assert.IsTrue(await taskService.Remove(TESTNAME, false, "", 0));

            // install a local tool here
            Assert.IsTrue(await taskService.Install(TESTNAME1, TESTVERSION1, "", false));
            // try global removal - fail
            Assert.IsFalse(await taskService.Remove(TESTNAME1, true, "", null));
            // correct removal
            Assert.IsTrue(await taskService.Remove(TESTNAME1, false, "", null));
            //Assert.IsTrue(await taskService.Remove(TESTNAME, false, GetCurrentDir(), null));

            Assert.AreEqual(0, libraryService.GetInstalled().Count());
        }

    }
}
