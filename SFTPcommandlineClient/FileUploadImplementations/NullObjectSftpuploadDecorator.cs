using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace DotNetSftp.FileUploadImplementations
{
    public class NullObjectSftpuploadDecorator : ISftpFileUploadDecorator
    {
        public void PerformPreUploadOperation(string pathOfFileToUpload, ref string destinationFilePath, SftpClient sftpClient, ILogger logger)
        {
            return;
        }

        public void UploadFile(string pathOfFileToUpload, ref string destinationFilePath, SftpClient sftpClient, ILogger logger)
        {
            return;
        }

        public void PerformPostUploadOperation(string pathOfFileToUpload,  ref string destinationFilePath, SftpClient sftpClient, ILogger logger)
        {
            return;
        }
    }
}