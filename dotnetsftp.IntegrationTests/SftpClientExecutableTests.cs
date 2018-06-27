using System;
using System.Collections.Generic;
using DotNetSftp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using dotnetsftp.IntegrationTests.TestUtilities;
using DotNetSftp.Settings;
using Renci.SshNet.Sftp;

namespace dotnetsftp.IntegrationTests
{
    [TestClass]
    public class SftpClientExecutableTests
    {
        private ConnectivitySettings connectivitySettings;

        [TestInitialize]
        public void InitializeTests()
        {
            connectivitySettings = SftpTestsHelperRoutines.CreateConnectivitySettingsFromConfig();
        }

        [TestMethod]
        public void TestCanExecuteProgramAndUploadSingleFile()
        {
            // arrange
            string pathOfTestFile = Path.GetTempFileName();
            string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} --username={connectivitySettings.UserName} --password={connectivitySettings.Password} --sp={pathOfTestFile} --dp=/home/user/foo/bar";
            string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

            // act/assert -- 'assert' as in 'doesn't throw an error'.
            Program.Main(commandlineArgsAsArray);
        }


        [TestMethod]
        [ExpectedException(typeof(SettingsValidationException))]
        public void TestCanExecuteProgramAndUploadSingleFile_throwsErrorOnBackwardSlashes()
        {
            // arrange
            string pathOfTestFile = Path.GetTempFileName();
            string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} --username={connectivitySettings.UserName} --password={connectivitySettings.Password} --sp={pathOfTestFile} --dp=\shouldnotIncludeBackwardSlash";
            string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

            // act/assert
            Program.Main(commandlineArgsAsArray);
        }

        [TestMethod]
        public void TestCanExecuteProgramAndUploadDictionary()
        {
            // arrange
            var sftpClient = SftpTestsHelperRoutines.GenerateBasicSftpClient();
            sftpClient.Connect();
            using (sftpClient)
            {
                string destinationPath = $@"/temp/{Guid.NewGuid().ToString()}";

                string testDirPath = Path.GetTempPath() + @"\foobar" + DateTime.Now.Ticks.ToString();
                if (!Directory.Exists(testDirPath))
                    Directory.CreateDirectory(testDirPath);
                string pathOfTestFile = Path.GetTempFileName();
                string fileName = Path.GetFileName(pathOfTestFile);
                string foo = Path.GetFullPath(testDirPath) + @"\" + fileName;
                File.Copy(pathOfTestFile, foo);


                string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} -username={connectivitySettings.UserName} -password={connectivitySettings.Password} -sp={testDirPath} -dp={destinationPath}";
                string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

                // act
                Program.Main(commandlineArgsAsArray);

                // assert
                IEnumerable<SftpFile> fileSystemEntries = sftpClient.ListDirectory(destinationPath);
                Assert.AreEqual(fileSystemEntries.Count(), 3);
            }
        }

        [TestMethod]
        public void TestCanExecuteProgramAndUploadDictionaryWithSubDir()
        {
            // arrange
            var sftpClient = SftpTestsHelperRoutines.GenerateBasicSftpClient();
            sftpClient.Connect();
            using (sftpClient)
            {
                string destinationPath = $@"/temp/{Guid.NewGuid().ToString()}";

                string testDirPath = Path.GetTempPath() + @"\foobar" + DateTime.Now.Ticks.ToString();
                if (!Directory.Exists(testDirPath))
                    Directory.CreateDirectory(testDirPath);
                string pathOfTestFile = Path.GetTempFileName();
                string fileName = Path.GetFileName(pathOfTestFile);
                File.Copy(pathOfTestFile, Path.GetFullPath(testDirPath) + @"\" + fileName);

                string testSubDirPath = testDirPath + @"\" + DateTime.Now.Ticks.ToString();
                if (!Directory.Exists(testSubDirPath))
                    Directory.CreateDirectory(testSubDirPath);
                File.Copy(pathOfTestFile, Path.GetFullPath(testSubDirPath) + @"\" + fileName);

                string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} -username={connectivitySettings.UserName} -password={connectivitySettings.Password} -sp={testDirPath} -dp={destinationPath}";
                string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

                // act
                Program.Main(commandlineArgsAsArray);

                // assert
                IEnumerable<SftpFile> fileSystemEntries = sftpClient.ListDirectory(destinationPath);
                Assert.AreEqual(fileSystemEntries.Count(), 4);
            }
        }


        [TestMethod]
        public void TestCanExecuteProgramAndUploadDictionaryWithSubDirs()
        {
            // arrange
            string testDirPath = Path.GetTempPath() + @"\foobar" + DateTime.Now.Ticks.ToString();
            if (!Directory.Exists(testDirPath))
                Directory.CreateDirectory(testDirPath);
            string pathOfTestFile = Path.GetTempFileName();
            string fileName = Path.GetFileName(pathOfTestFile);
            string foo = Path.GetFullPath(testDirPath) + @"\" + fileName;
            File.Copy(pathOfTestFile, foo);

            string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} -username={connectivitySettings.UserName} -password={connectivitySettings.Password} -sp={testDirPath} -dp=/home/user/foo/bar";
            string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

            // act/assert -- 'assert' as in 'doesn't throw an error'.
            Program.Main(commandlineArgsAsArray);
        }


        [TestMethod]
        public void TestCanExecuteProgramAndUploadSingleFile_overwritesExisting()
        {
            // arrange
            string pathOfTestFile = Path.GetTempFileName();
            string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} -username={connectivitySettings.UserName} -password={connectivitySettings.Password} -sp={pathOfTestFile} -dp=/home/user/foo/bar -ow";
            string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

            // act/assert -- 'assert' as in 'doesn't throw an error'.
            Program.Main(commandlineArgsAsArray); // upload file once ...
            Program.Main(commandlineArgsAsArray); // ... then upload it again, to ensure the overwrite-functionality works as intended.
        }

        [TestMethod]
        public void TestCanExecuteProgramAndUploadSingleFile_withUploadPrefix()
        {
            // arrange
            string pathOfTestFile = Path.GetTempFileName();
            string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} -username={connectivitySettings.UserName} -password={connectivitySettings.Password} -sp={pathOfTestFile} -dp=/home/user/foo/bar -up=.";
            string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

            // act/assert -- 'assert' as in 'doesn't throw an error'.
            Program.Main(commandlineArgsAsArray);
        }

        [TestMethod]
        public void TestCanExecuteProgramAndUploadSingleFile_withCheckSumFile()
        {
            // arrange
            string pathOfTestFile = Path.GetTempFileName();
            string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} -username={connectivitySettings.UserName} -password={connectivitySettings.Password} -sp={pathOfTestFile} -dp=/home/user/foo/bar -cs";
            string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

            // act/assert -- 'assert' as in 'doesn't throw an error'.
            Program.Main(commandlineArgsAsArray);
        }

        [TestMethod]
        public void TestCanExecuteProgramAndUploadSingleFile_withCheckSumFileAndUploadPrefix()
        {
            // arrange
            string pathOfTestFile = Path.GetTempFileName();
            string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} -username={connectivitySettings.UserName} -password={connectivitySettings.Password} -sp={pathOfTestFile} -dp=/home/user/foo/bar -cs -up=.";
            string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

            // act/assert -- 'assert' as in 'doesn't throw an error'.
            Program.Main(commandlineArgsAsArray);
        }

        [TestMethod]
        public void TestCanExecuteProgramAndUploadSingleFile_withPrivateKeyAuthentication()
        {
            // arrange
            string pathOfTestFile = Path.GetTempFileName();
            string pathOfPrivateKeyFile = Path.Combine(TestFilesHelper.GetTestFileDir(), "privateKeyOpenSSH.key");
            string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} -username={connectivitySettings.UserName} -pk={pathOfPrivateKeyFile} -password={connectivitySettings.Password} -sp={pathOfTestFile} -dp=/home/user/foo/bar -ow -vt";
            string[] commandlineArgsAsArray = commandlineArgs.Split(' ');


            // act/assert -- 'assert' as in 'doesn't throw an error'.
            Program.Main(commandlineArgsAsArray);
        }


        [TestMethod]
        public void TestCanExecuteProgramAndUploadDictionaryWithCompression()
        {
            // arrange
            var sftpClient = SftpTestsHelperRoutines.GenerateBasicSftpClient();
            sftpClient.Connect();
            using (sftpClient)
            {
                string destinationPath = $@"/temp/{Guid.NewGuid().ToString()}";

                string testDirPath = Path.GetTempPath() + @"\foobar" + DateTime.Now.Ticks.ToString();
                if (!Directory.Exists(testDirPath))
                    Directory.CreateDirectory(testDirPath);
                string pathOfTestFile = Path.GetTempFileName();
                string fileName = Path.GetFileName(pathOfTestFile);
                File.Copy(pathOfTestFile, Path.GetFullPath(testDirPath) + @"\" + fileName);

                string testSubDirPath = testDirPath + @"\" + DateTime.Now.Ticks.ToString();
                if (!Directory.Exists(testSubDirPath))
                    Directory.CreateDirectory(testSubDirPath);
                File.Copy(pathOfTestFile, Path.GetFullPath(testSubDirPath) + @"\" + fileName);

                string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} -username={connectivitySettings.UserName} -password={connectivitySettings.Password} -sp={testDirPath} -dp={destinationPath} -cd";
                string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

                // act
                Program.Main(commandlineArgsAsArray);

                // assert
                IEnumerable<SftpFile> fileSystemEntries = sftpClient.ListDirectory(destinationPath);
                Assert.AreEqual(fileSystemEntries.Count(), 3); // 3 entries - 1 zip file, one '.' home-dir, one '..' previous dir.
            }
        }

        [TestMethod]
        public void TestCanExecuteProgramAndSaveSettingsFile()
        {
            // arrange
            string pathOfTestFile = Path.GetTempFileName();
            string pathOfTestSettingsFile = Path.GetTempPath() + DateTime.Now.Ticks.ToString() + ".settings";
            string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} --username={connectivitySettings.UserName} --password={connectivitySettings.Password} --sp={pathOfTestFile} --dp=/home/user/foo/bar --sf={pathOfTestSettingsFile}";
            string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

            // act
            Program.Main(commandlineArgsAsArray);

            // assert
            bool settingsFileWasWritten = File.Exists(pathOfTestSettingsFile);
            Assert.IsTrue(settingsFileWasWritten);
        }

        [TestMethod]
        public void TestCanExecuteProgramAndSaveSettingsFileAndEncryptItAndSaveEncryptionKeyFile()
        {
            // arrange
            string pathOfTestFile = Path.GetTempFileName();
            string pathOfTestSettingsFile = Path.GetTempPath() + DateTime.Now.Ticks.ToString() + ".settings";
            string pathOfTestSettingsEncryptionFile = Path.GetTempPath() + DateTime.Now.Ticks.ToString() + ".encryptionKey";
            string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} --username={connectivitySettings.UserName} --password={connectivitySettings.Password} --sp={pathOfTestFile} --dp=/home/user/foo/bar --sf={pathOfTestSettingsFile} --sfKey={pathOfTestSettingsEncryptionFile}";
            string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

            // act
            Program.Main(commandlineArgsAsArray);

            // assert
            bool settingsFileWasWritten = File.Exists(pathOfTestSettingsFile);
            Assert.IsTrue(settingsFileWasWritten);
        }

        [TestMethod]
        public void TestCanExecuteProgramAndSaveSettingsAndReadItBack()
        {
            // arrange
            string pathOfTestFile = Path.GetTempFileName();
            string pathOfTestSettingsFile = Path.GetTempPath() + DateTime.Now.Ticks.ToString() + ".settings";
            string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} --username={connectivitySettings.UserName} --password={connectivitySettings.Password} --sp={pathOfTestFile} --dp=/home/user/foo/bar  --sf={pathOfTestSettingsFile}";
            string[] commandlineArgsAsArray = commandlineArgs.Split(' ');
            Program.Main(commandlineArgsAsArray);

            // act
            commandlineArgs = $@"--sf={pathOfTestSettingsFile}";
            commandlineArgsAsArray = commandlineArgs.Split(' ');
            Program.Main(commandlineArgsAsArray);

            // assert
            bool settingsFileWasWritten = File.Exists(pathOfTestSettingsFile);
            Assert.IsTrue(settingsFileWasWritten);
        }

        [TestMethod]
        public void TestWontOverwriteFileOfSameSize()
        {
            // arrange
            var sftpClient = SftpTestsHelperRoutines.GenerateBasicSftpClient();
            sftpClient.Connect();
            using (sftpClient)
            {
                string destinationPath = $@"/temp/{Guid.NewGuid().ToString()}";

                string testDirPath = Path.GetTempPath() + @"\foobar" + DateTime.Now.Ticks.ToString();
                if (!Directory.Exists(testDirPath))
                    Directory.CreateDirectory(testDirPath);
                string pathOfTestFile = Path.GetTempFileName();
                File.WriteAllText(pathOfTestFile, "Hello World");
                string fileName = Path.GetFileName(pathOfTestFile);
                string foo = Path.GetFullPath(testDirPath) + @"\" + fileName;
                File.Copy(pathOfTestFile, foo);


                string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} -username={connectivitySettings.UserName} -password={connectivitySettings.Password} -sp={testDirPath} -dp={destinationPath} -ow";
                string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

                // act
                DateTime firstFileWriteTimeStamp = DateTime.Now.ToUniversalTime();
                Program.Main(commandlineArgsAsArray); // first upload
                Program.Main(commandlineArgsAsArray); // second upload - shouldn't overwrite the first
                DateTime secondFileWriteTimeStamp = DateTime.Now.ToUniversalTime();

                // assert
                var uploadedFileTimeStamp = sftpClient.GetLastWriteTimeUtc(destinationPath + @"\" + fileName);
                Assert.IsTrue(uploadedFileTimeStamp > firstFileWriteTimeStamp);
                Assert.IsTrue(uploadedFileTimeStamp < secondFileWriteTimeStamp);
            }
        }

        [TestMethod]
        public void TestWontOverwriteFileOfDifferentSize()
        {
            // arrange
            var sftpClient = SftpTestsHelperRoutines.GenerateBasicSftpClient();
            sftpClient.Connect();
            using (sftpClient)
            {
                string destinationPath = $@"/temp/{Guid.NewGuid().ToString()}";

                string testDirPath = Path.GetTempPath() + @"\foobar" + DateTime.Now.Ticks.ToString();
                if (!Directory.Exists(testDirPath))
                    Directory.CreateDirectory(testDirPath);
                string pathOfTestFile = Path.GetTempFileName();
                File.WriteAllText(pathOfTestFile, "Hello World");
                string fileName = Path.GetFileName(pathOfTestFile);

                string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} -username={connectivitySettings.UserName} -password={connectivitySettings.Password} -sp={pathOfTestFile} -dp={destinationPath} -ow";
                string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

                // act
                DateTime firstFileWriteTimeStamp = DateTime.Now.ToUniversalTime();
                Program.Main(commandlineArgsAsArray); // first upload
                File.WriteAllText(pathOfTestFile, "Hello World - more text");
                Program.Main(commandlineArgsAsArray); // second upload - should overwrite the first
                DateTime secondFileWriteTimeStamp = DateTime.Now.ToUniversalTime();

                // assert
                var uploadedFileTimeStamp = sftpClient.GetLastWriteTimeUtc(destinationPath + @"\" + fileName);
                Assert.IsTrue(uploadedFileTimeStamp > firstFileWriteTimeStamp);
                Assert.IsTrue(uploadedFileTimeStamp < secondFileWriteTimeStamp);
            }
        }


        [TestMethod]
        public void TestWontOverwriteFileOfNewerTimeStamp()
        {
            // arrange
            var sftpClient = SftpTestsHelperRoutines.GenerateBasicSftpClient();
            sftpClient.Connect();
            using (sftpClient)
            {
                string destinationPath = $@"/temp/{Guid.NewGuid().ToString()}";

                string testDirPath = Path.GetTempPath() + @"\foobar" + DateTime.Now.Ticks.ToString();
                if (!Directory.Exists(testDirPath))
                    Directory.CreateDirectory(testDirPath);
                string pathOfTestFile = Path.GetTempFileName();
                File.WriteAllText(pathOfTestFile, "Hello World");
                string fileName = Path.GetFileName(pathOfTestFile);
                string foo = Path.GetFullPath(testDirPath) + @"\" + fileName;
                File.Copy(pathOfTestFile, foo);

                string commandlineArgs = $@"--tt=upload --host={connectivitySettings.Host} -username={connectivitySettings.UserName} -password={connectivitySettings.Password} -sp={pathOfTestFile} -dp={destinationPath} -ow";
                string[] commandlineArgsAsArray = commandlineArgs.Split(' ');

                // act
                DateTime firstFileWriteTimeStamp = DateTime.Now.ToUniversalTime();
                Program.Main(commandlineArgsAsArray); // first upload
                System.Threading.Thread.Sleep(2000);
                File.WriteAllText(pathOfTestFile, "Hello World"); // update file's changed-timestamp
                Program.Main(commandlineArgsAsArray); // second upload - is newer, should overwrite the first on the server

                // assert
                var uploadedFileTimeStamp = sftpClient.GetLastWriteTimeUtc(destinationPath + @"\" + fileName);
                Assert.IsTrue(firstFileWriteTimeStamp < uploadedFileTimeStamp);
            }
        }


    }
}