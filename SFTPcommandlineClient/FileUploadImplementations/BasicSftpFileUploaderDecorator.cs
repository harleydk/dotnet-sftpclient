using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System.IO;

namespace DotNetSftp.FileUploadImplementations
{
    public class BasicSftpFileUploaderDecorator : SftpFileUploadDecoratorBase
    {
        public BasicSftpFileUploaderDecorator(ISftpFileUploadDecorator sftpFileUploadDecorator) : base(
            sftpFileUploadDecorator)
        {
        }

        public override void UploadFile(string pathOfFileToUpload, ref string destinationFilePath, SftpClient sftpClient, ILogger logger)
        {
            base.UploadFile(pathOfFileToUpload, ref destinationFilePath, sftpClient, logger);

            if (destinationFilePath.Contains(@"/"))
            {
                string destinationDirectory = destinationFilePath.Substring(0, destinationFilePath.LastIndexOf('/'));
                sftpClient.ChangeDirectory(destinationDirectory);
            }

            logger.LogDebug($"Will upload {pathOfFileToUpload}");
            using (FileStream fs = new FileStream(pathOfFileToUpload, FileMode.Open))
            {
                sftpClient.BufferSize = 4 * 1024;
                sftpClient.UploadFile(fs, destinationFilePath,
                    true /* Automatically overwrite existing. We've already, elsewhere, made that decision. */);
            }
        }
    }
}