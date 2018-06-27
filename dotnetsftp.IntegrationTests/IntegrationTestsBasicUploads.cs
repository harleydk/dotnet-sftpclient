using DotNetSftp;
using DotNetSftp.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.IO;
using dotnetsftp.IntegrationTests.TestUtilities;

namespace dotnetsftp.IntegrationTests
{
    [TestClass]
    public class IntegrationTestsBasicUploads
    {
        private ILogger testLogger = new TestLogger();

        /// <summary>
        /// Test connectivity against free online sftp-server
        /// </summary>
        [TestMethod]
        public void OnlineConnectivityTest_canConnect_usernameAndPassword()
        {
            // arrange
            ConnectivitySettings connectivitySettings = SftpTestsHelperRoutines.CreateConnectivitySettingsFromConfig();
            connectivitySettings.PrivateKeyPath = string.Empty;
            DotNetSftpClient dotNetSftpClient = new DotNetSftpClient(connectivitySettings, testLogger);

            // act/assert - 'assert' as in no exceptions are thrown
            using (dotNetSftpClient)
            {
                dotNetSftpClient.CreateConnection();
            }
        }

        /// <summary>
        /// Test connectivity against free online sftp-server
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(DotNetSftpClientException))]
        public void OnlineConnectivityTest_ThrowsConnectionError()
        {
            // arrange
            ConnectivitySettings connectivitySettings = SftpTestsHelperRoutines.CreateConnectivitySettingsFromConfig();
            connectivitySettings.Host = "ImPrettySureThisHostDoesntExistInAnyShapeOrForm";
            connectivitySettings.PrivateKeyPath = string.Empty;
            DotNetSftpClient dotNetSftpClient = new DotNetSftpClient(connectivitySettings, testLogger);

            // act/assert - 'assert' as in no exceptions are thrown
            using (dotNetSftpClient)
            {
                dotNetSftpClient.CreateConnection();
            }
        }

        [TestMethod()]
        public void UploadFileTest_usernamePasswordConnection_canUploadFile()
        {
            // arrange
            ConnectivitySettings connectivitySettings = SftpTestsHelperRoutines.CreateConnectivitySettingsFromConfig();
            connectivitySettings.PrivateKeyPath = String.Empty; // clear private key path, so we only connect via username/password
            DotNetSftpClient dotNetSftpClient = new DotNetSftpClient(connectivitySettings, testLogger);
            dotNetSftpClient.CreateConnection();

            string newTempFilePath = Path.GetTempFileName();
            TransferSettings transferSettings = new TransferSettings()
            {
                TransferType = 'u',
                DestinationPath = " ",
                SourcePath = newTempFilePath
            };

            // act/assert - 'assert' as in no exceptions are thrown
            using (dotNetSftpClient)
            {
                dotNetSftpClient.Upload(transferSettings);
            }
        }

        /// <summary>
        /// Validates a connection to the sftp-server using a private key.
        /// </summary>
        /// <remarks>
        /// In order to connect, the private key must be authenticated against a public key on the server.
        /// Copy the 'publicKey.pub' from the 'TestFiles'-folder to the server.
        /// </remarks>
        [TestMethod()]
        public void OnlineConnectivityTest_privateKeyConnection_canConnect()
        {
            // arrange
            ConnectivitySettings connectivitySettings = SftpTestsHelperRoutines.CreateConnectivitySettingsFromConfig();
            string physicalPathToPrivateKeyFile = SftpTestsHelperRoutines.GetTestFilesPath() + connectivitySettings.PrivateKeyPath;
            connectivitySettings.PrivateKeyPath = physicalPathToPrivateKeyFile;

            // act/assert - 'assert' as in no exceptions are thrown
            DotNetSftpClient dotNetSftpClient = new DotNetSftpClient(connectivitySettings, testLogger);
            using (dotNetSftpClient)
            {
                dotNetSftpClient.CreateConnection();
            }
        }

        [TestMethod]
        public void UploadFileTest_privateKeyConnection_canUploadFile()
        {
            // arrange
            ConnectivitySettings connectivitySettings = SftpTestsHelperRoutines.CreateConnectivitySettingsFromConfig();
            string physicalPathToPrivateKeyFile = SftpTestsHelperRoutines.GetTestFilesPath() + connectivitySettings.PrivateKeyPath;
            connectivitySettings.PrivateKeyPath = physicalPathToPrivateKeyFile;

            DotNetSftpClient dotNetSftpClient = new DotNetSftpClient(connectivitySettings, testLogger);
            dotNetSftpClient.CreateConnection();

            // act/assert - 'assert' as in no exceptions are thrown
            string newTempFilePath = Path.GetTempFileName();
            TransferSettings transferSettings = new TransferSettings()
            {
                TransferType = 'u',
                DestinationPath = string.Empty,
                SourcePath = newTempFilePath
            };

            // act/assert - 'assert' as in no exceptions are thrown
            using (dotNetSftpClient)
            {
                dotNetSftpClient.Upload(transferSettings);
            }
        }

    }
}