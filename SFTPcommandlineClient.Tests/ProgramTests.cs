using System;
using System.IO;
using DotNetSftp;
using DotNetSftp.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dotnetsftp.UnitTests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        [ExpectedException(typeof(SettingsValidationException))]
        public void TestCanRunClient_tooFewArguments_onlyHostName()
        {
            // arrange
            string[] commandlineArguments = new string[]
            {
                "--tt=upload --host=localhost"
            };

            // act
            Program.Main(commandlineArguments);
        }

        [TestMethod]
        [ExpectedException(typeof(SettingsValidationException))]
        public void TestCanRunClient_tooFewArguments_onlyClientKeyPath()
        {
            // arrange
            string[] commandlineArguments = new string[]
            {
                "--tt=upload --host=localhost",
                @"-pk=c:\temp",
            };

            // act
            Program.Main(commandlineArguments);
        }

        [TestMethod()]
        public void TestCanSaveSettingsToFile()
        {
            // arrange
            SettingsParser settingsParser = new SettingsParser();
            string tempSettingsFile = Path.GetTempFileName();

            // act
            Program.SaveSettingsToFile(settingsParser, tempSettingsFile, NullLogger.Instance);

            // assert
            Assert.IsTrue(File.Exists(tempSettingsFile));
        }

        [TestMethod()]
        public void SaveSettingsToFileTest()
        {
            // arrange
            SettingsParser settingsParser = new SettingsParser();
            string tempSettingsFile = Path.GetTempPath() + DateTime.Now.Ticks.ToString();
            string tempSettingsKeyFilePath = Path.GetTempPath() + $"{DateTime.Now.Ticks.ToString()}.encryptionKey";

            // act
            Program.SaveSettingsToFile(settingsParser, tempSettingsFile, NullLogger.Instance, tempSettingsKeyFilePath);

            // assert
            Assert.IsTrue(File.Exists(tempSettingsFile));
            Assert.IsTrue(File.Exists(tempSettingsKeyFilePath));
            string encryptionKeyFromFile = File.ReadAllText(tempSettingsKeyFilePath);
            Assert.AreEqual(encryptionKeyFromFile.Length, 8 /* length of a guid - we generate guid's as encryption keys, though restricting to 8 because of our choice of encryption-algorithm. */); 
        }

        [TestMethod()]
        public void TestCanImportSettingsFromUnencryptedFile()
        {
            // arrange
            SettingsParser settingsParser = new SettingsParser();
            settingsParser.TransferSettings.TransferType = 'u';
            settingsParser.ConnectivitySettings.Host = @"foobarHost";
            string tempSettingsFile = Path.GetTempPath() + DateTime.Now.Ticks.ToString();
            Program.SaveSettingsToFile(settingsParser, tempSettingsFile, NullLogger.Instance);

            // act
            SettingsParser readSettings = Program.ImportSettingsFromFile(tempSettingsFile, NullLogger.Instance);

            // assert
            Assert.IsNotNull(readSettings);
            Assert.AreEqual(readSettings.TransferSettings.TransferType, 'u');
            Assert.AreEqual(readSettings.ConnectivitySettings.Host, "foobarHost");
        }

        [TestMethod()]
        public void TestCanImportSettingsFromEncryptedFile()
        {
            // arrange
            SettingsParser settingsParser = new SettingsParser();
            settingsParser.TransferSettings.TransferType = 'd';
            settingsParser.ConnectivitySettings.Host = @"barfooHost";
            string tempSettingsFile = Path.GetTempPath() + DateTime.Now.Ticks.ToString();
            string tempSettingsKeyFilePath = Path.GetTempPath() + $"{DateTime.Now.Ticks.ToString()}.encryptionKey";
            Program.SaveSettingsToFile(settingsParser, tempSettingsFile, NullLogger.Instance, tempSettingsKeyFilePath);

            // act
            SettingsParser readSettings = Program.ImportSettingsFromFile(tempSettingsFile, NullLogger.Instance, tempSettingsKeyFilePath);

            // assert
            Assert.IsNotNull(readSettings);
            Assert.AreEqual(readSettings.TransferSettings.TransferType, 'd');
            Assert.AreEqual(readSettings.ConnectivitySettings.Host, "barfooHost");
        }

    }
}