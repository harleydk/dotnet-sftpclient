using System.IO;
using dotnetsftp.IntegrationTests.TestUtilities;
using DotNetSftp.FileUploadImplementations;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dotnetsftp.IntegrationTests
{
    [TestClass]
    public class IntegrationTestsChecksumIndicatorDecorator
    {
        private ILogger testLogger = new TestLogger();

        [TestMethod]
        public void TestCanUploadUsingChecksumDecorator()
        {
            // arrange
            var sftpClient = SftpTestsHelperRoutines.GenerateBasicSftpClient();
            BasicSftpFileUploaderDecorator uploaderDecoratorWithChecksumDecoration =
                new BasicSftpFileUploaderDecorator(
                    new ChecksumFileDecorator(new NullObjectSftpuploadDecorator()));

            StandardSftpFileUploader standardSftpFileUploader = new StandardSftpFileUploader(uploaderDecoratorWithChecksumDecoration, testLogger);

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
                string fileName = Path.GetFileName(newTempFilePath);
                string filenameExtension = Path.GetExtension(fileName);
                sftpClient.DeleteFile(fileName);
                sftpClient.DeleteFile(fileName + ".sha256");
            }
        }


    }
}