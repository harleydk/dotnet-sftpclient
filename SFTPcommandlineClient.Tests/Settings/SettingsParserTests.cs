using DotNetSftp.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetSftp.Settings.Tests
{
    [TestClass()]
    public class SettingsParserTests
    {
        [TestMethod()]
        public void ParseSettingsFromCommandlineArgumentsTest_tooFewConnectionArguments()
        {
            // arrange
            SettingsParser parser = new SettingsParser();
            string[] commandlineArguments = new string[] { "-host=localhost", "-d" };

            // act
            parser.ParseSettingsFromCommandlineArguments(commandlineArguments);

            // assert
            Assert.AreEqual(parser.ConnectivitySettings.Host, "localhost");
        }

        [TestMethod()]
        public void ParseSettingsFromCommandlineArgumentsTest_parsesPortNrCorrectly()
        {
            // arrange
            SettingsParser parser = new SettingsParser();
            string[] commandlineArguments = new string[] { "--host=127.0.0.1", "--port=2222" };

            // act
            parser.ParseSettingsFromCommandlineArguments(commandlineArguments);

            // assert
            Assert.AreEqual(parser.ConnectivitySettings.Port, 2222);
            Assert.AreNotEqual(parser.ConnectivitySettings.Port, 0);
            Assert.AreNotEqual(parser.ConnectivitySettings.Port, 23);
        }

    }
}