using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace DotNetSftp.FileUploadImplementations
{
    public interface ISftpFileUploadDecorator
    {
        void PerformPreUploadOperation(string pathOfFileToUpload, ref string destinationFilePath, SftpClient sftpClient, ILogger logger);
        void UploadFile(string pathOfFileToUpload, ref string destinationFilePath, SftpClient sftpClient, ILogger logger);
        void PerformPostUploadOperation(string pathOfFileToUpload, ref string destinationFilePath, SftpClient sftpClient, ILogger logger);


    }
}
