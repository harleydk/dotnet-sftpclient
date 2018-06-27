using System.IO;
using dotnetsftp.IntegrationTests.TestUtilities;
using DotNetSftp.FileUploadImplementations;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dotnetsftp.IntegrationTests
{
    [TestClass]
    public class IntegrationTestsTemporaryIndicatorDecorator
    {
        private ILogger testLogger = new TestLogger();

        [TestMethod]
        public void TestCanUploadUsingTemporaryIndicatorDecorator()
        {
            // arrange
            
            var sftpClient = SftpTestsHelperRoutines.GenerateBasicSftpClient();
            BasicSftpFileUploaderDecorator uploaderDecoratorWithTemporaryFilesUploadDecoration =
                new BasicSftpFileUploaderDecorator(
                    new UploadPrefixIndicatorDecorator("foobar", new NullObjectSftpuploadDecorator()));

            StandardSftpFileUploader standardSftpFileUploader = new StandardSftpFileUploader(uploaderDecoratorWithTemporaryFilesUploadDecoration, testLogger);

            // act
            sftpClient.Connect();
            using (sftpClient)
            {
                string newTempFilePath = Path.GetTempFileName();
                standardSftpFileUploader.PerformFileUpload(newTempFilePath, $@"{Path.GetFileName(newTempFilePath)}", sftpClient, testLogger);

                // assert
                bool fileWasUploadedAndRenamed = sftpClient.Exists(Path.GetFileName(newTempFilePath));
                Assert.IsTrue(fileWasUploadedAndRenamed);

                // clean up
                sftpClient.DeleteFile(Path.GetFileName(newTempFilePath));
            }
        }
    }
}