using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using gpm.core.Services;
using gpm.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        private const string TESTSLOT = "/Users/ghost/gpm2";

        public CommandLineTests()
        {
            _host = GenericHost.CreateHostBuilder(null).Build();
        }

        [TestCleanup]
        public void Cleanup()
        {
            LogBeginOfTest();

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            dataBaseService.FetchAndUpdateSelf();

            Remove.Action(TESTNAME, 0, true, _host);
            Remove.Action(TESTNAME2, 0, true, _host);
        }

        [TestMethod]
        public async Task TestAll()
        {
            LogBeginOfTest();

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            dataBaseService.FetchAndUpdateSelf();

            TestUpgrade();

            TestList();

            // cleanup
            TestRemove();
            TestInstalled();

            await TestInstall();

            await TestUpdate();

            TestInstalled();

            TestRemove();
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
            dataBaseService.FetchAndUpdateSelf();

            dataBaseService.ListAllPackages();
        }

        [TestMethod]
        public void TestInstalled()
        {
            LogBeginOfTest();

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

            dataBaseService.FetchAndUpdateSelf();

            Installed.Action("", "", _host);
        }

        [TestMethod]
        public async Task TestInstall()
        {
            LogBeginOfTest();

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            //var logger = serviceProvider.GetRequiredService<ILogger<CommandLineTests>>();
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            dataBaseService.FetchAndUpdateSelf();

            // test installing a nonexisting package
            await Install.Action(TESTNAMEWRONG, "", "", _host);

            // test installing latest -> PASS
            await Install.Action(TESTNAME, "", "", _host);
            // test installing again -> FAIL
            await Install.Action(TESTNAME, "", "", _host);
            // test installing a wrong version -> FAIL
            await Install.Action(TESTNAME, TESTVERSIONWRONG, "", _host);
            // test installing a previous version into default slot  -> FAIL
            await Install.Action(TESTNAME, TESTVERSION2, "", _host);

            // test installing a previous version into new slot -> PASS
            await Install.Action(TESTNAME, TESTVERSION1, TESTSLOT, _host);
            // test installing another version into new slot -> FAIL
            await Install.Action(TESTNAME, TESTVERSION2, TESTSLOT, _host);


            // test installing another repo into default slot -> PASS
            await Install.Action(TESTNAME2, "", "", _host);
            // test installing another repo over an existing default slot -> FAIL
            await Install.Action(TESTNAME2, "", TESTSLOT, _host);

        }

        [TestMethod]
        public async Task TestUpdate()
        {
            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            dataBaseService.FetchAndUpdateSelf();

            // test updating default
            await Update.Action(TESTNAME, false, 0, false, _host);

            // test updating default
            await Update.Action(TESTNAME, false, 0, false, _host);

            // test updating all
            await Update.Action(TESTNAME, false, 0, false, _host);


        }

        [TestMethod]
        public void TestRemove()
        {
            LogBeginOfTest();

            var serviceProvider = _host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

            dataBaseService.FetchAndUpdateSelf();

            Remove.Action(TESTNAME, 1, false, _host);

            Remove.Action(TESTNAME, 0, true, _host);
        }


        private static void LogBeginOfTest([CallerMemberName] string methodName = "") => Console.WriteLine(methodName);
    }
}
