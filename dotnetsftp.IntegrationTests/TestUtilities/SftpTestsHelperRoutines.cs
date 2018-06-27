using System;
using System.Configuration;
using System.IO;
using DotNetSftp.Settings;
using Renci.SshNet;

namespace dotnetsftp.IntegrationTests.TestUtilities
{
    /// <summary>
    /// The integration tests must be executed against a localhost sftp-server. This piece of code ensures it's up and running.
    /// </summary>
    /// <remarks>
    /// Included in the SolutionItems folder is a free sftp-server, RebexTinySftpServer, that can be used with integration testing.
    /// Remember, though, about public-/private-key authentication: In order to connect, the private key must be authenticated against a public key on the server.
    /// Copy the 'publicKey.pub' from the 'TestFiles'-folder to the server.
    /// </remarks>
    public class SftpTestsHelperRoutines
    {
        public static bool IsLocalSftpServerRunning()
        {
            string sftpServerHostAddress = ConfigurationManager.AppSettings["sftpServerHostAddress"];
            int sftpServerPortNo = Convert.ToInt32(ConfigurationManager.AppSettings["sftpServerPortNo"]);

            using (PrimS.Telnet.Client telnetClient = new PrimS.Telnet.Client(sftpServerHostAddress, sftpServerPortNo, new System.Threading.CancellationToken()))
            {
                return telnetClient.IsConnected;
            }
        }

        /// <summary>
        /// Get the connection-info for the sftp-server from the config.
        /// If no sftp-server is available for integration-tests within the organization, one option for local testing is the free 'tinysftpserver' from Rebex.
        /// </summary>
        /// <returns>A fully loaded ConnectivitySettings-object.</returns>
        public static ConnectivitySettings CreateConnectivitySettingsFromConfig()
        {
            string host = ConfigurationManager.AppSettings["sftpServerHostAddress"];
            string port = ConfigurationManager.AppSettings["sftpServerPortNo"];
            string userName = ConfigurationManager.AppSettings["userName"];
            string password = ConfigurationManager.AppSettings["password"];
            string privateKeyPath = ConfigurationManager.AppSettings["privateKeyPath"];

            ConnectivitySettings connectivitySettings = new ConnectivitySettings()
            {
                Host = host,
                Port = Convert.ToInt32(port),
                UserName = userName,
                Password = password,
                PrivateKeyPath = privateKeyPath
            };

            return connectivitySettings;
        }

        public static string GetTestFilesPath()
        {
            DirectoryInfo testDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            testDir = new DirectoryInfo(testDir.Parent.Parent.FullName + @"\TestFiles\");
            return testDir.FullName + @"\";
        }

        public static SftpClient GenerateBasicSftpClient()
        {
            ConnectivitySettings connectivitySettingsFromConfig = CreateConnectivitySettingsFromConfig();
            SftpClient testSftpClient = new SftpClient(connectivitySettingsFromConfig.Host, connectivitySettingsFromConfig.Port, connectivitySettingsFromConfig.UserName, connectivitySettingsFromConfig.Password);
            return testSftpClient;
        }

    }

}