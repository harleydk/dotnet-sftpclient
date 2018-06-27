using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace DotNetSftp.FileUploadImplementations
{
    /// <summary>
    /// Interface for FileUploader-implementations.
    /// Offers an extensibility-point for other implementations.
    /// </summary>
    public interface ISftpFileUploader
    {
        void PerformFileUpload(string pathOfFileToUpload, string destinationFilePath, SftpClient sftpClient, ILogger logger);
    }
}