using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace DotNetSftp.FileUploadImplementations
{
    /// <summary>
    /// Standard sftp-file uploader. Accpets an sft-file upload decorator implementation.
    /// </summary>
    /// <remarks>
    /// The decorator design pattern is a software design pattern that allows us to extend the functionality of existing classes. In this case we 
    /// can use it to add various enhancements to the upload - for example verification of the upload, or the creation of a signature. 
    /// </remarks>
    public class StandardSftpFileUploader : ISftpFileUploader
    {
        private readonly ILogger _logger;
        private readonly ISftpFileUploadDecorator _sftpFileUploadDecorator;

        public StandardSftpFileUploader(ISftpFileUploadDecorator sftpFileUploadDecorator, ILogger logger)
        {
            _logger = logger;
            _sftpFileUploadDecorator = sftpFileUploadDecorator;
        }

        /// <summary>
        /// Perform the upload. This will call into the decorator, we're given in the constructor. 
        /// Remember, the decorator may contains several other decorators, that will, in turn, be called.
        /// </summary>
        public void PerformFileUpload(string pathOfFileToUpload, string destinationFilePath, SftpClient sftpClient, ILogger logger)
        {
            _sftpFileUploadDecorator.PerformPreUploadOperation(pathOfFileToUpload, ref destinationFilePath,sftpClient,logger);
            _sftpFileUploadDecorator.UploadFile(pathOfFileToUpload, ref destinationFilePath, sftpClient, logger);
            _sftpFileUploadDecorator.PerformPostUploadOperation(pathOfFileToUpload, ref destinationFilePath, sftpClient, logger);
        }
    }
}