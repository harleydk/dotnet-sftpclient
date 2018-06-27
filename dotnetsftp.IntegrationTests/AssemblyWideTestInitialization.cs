using dotnetsftp.IntegrationTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace dotnetsftp.IntegrationTests
{
    [TestClass]
    public class AssemblyWideTestInitialization
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            bool isTelnetServerRunning = SftpTestsHelperRoutines.IsLocalSftpServerRunning();
            if (!isTelnetServerRunning)
                throw new Exception("Local SFTP-server is not running, and we need that for these integration-tests.");
        }
    }
}