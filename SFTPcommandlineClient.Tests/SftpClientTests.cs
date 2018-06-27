using DotNetSftp.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetSftp.Tests
{
    [TestClass()]
    public class SftpClientTests
    {
        [TestMethod()]
        [ExpectedException(typeof(DotNetSftpClientException))]
        public void CreateConnectionTest_tooFewConnectionSettings()
        {
            // arrange
            ConnectivitySettings connectivitySettings = new ConnectivitySettings();
            connectivitySettings.Host = "foobar";
            DotNetSftpClient dotNetSftpClient = new DotNetSftpClient(connectivitySettings, NullLogger.Instance);

            // act/assert
            dotNetSftpClient.CreateConnection();
        }
    }
}