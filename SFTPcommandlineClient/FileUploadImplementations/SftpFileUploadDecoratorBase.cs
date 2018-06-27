using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace DotNetSftp.FileUploadImplementations
{
    public abstract class SftpFileUploadDecoratorBase : ISftpFileUploadDecorator
    {
        private readonly ISftpFileUploadDecorator _sftpFileUploadDecorator;

        protected SftpFileUploadDecoratorBase(ISftpFileUploadDecorator sftpFileUploadDecorator)
        {
            _sftpFileUploadDecorator = sftpFileUploadDecorator;
        }

        public virtual void PerformPreUploadOperation(string pathOfFileToUpload, ref string destinationFilePath, SftpClient sftpClient, ILogger logger)
        {
            System.Diagnostics.Debug.WriteLine($"Called PerformPreUploadOperation() on {this.GetType().Name}");
            logger.LogDebug($"Called PerformPreUploadOperation() on {this.GetType().Name}");

            if (!(_sftpFileUploadDecorator is NullObjectSftpuploadDecorator))
                _sftpFileUploadDecorator.PerformPreUploadOperation(pathOfFileToUpload, ref destinationFilePath, sftpClient, logger);
        }

        public virtual void UploadFile(string pathOfFileToUpload, ref string destinationFilePath, SftpClient sftpClient, ILogger logger)
        {
            System.Diagnostics.Debug.WriteLine($"Called UploadFile() on {this.GetType().Name}");
            logger.LogDebug($"Called UploadFile() on {this.GetType().Name}");

            if (!(_sftpFileUploadDecorator is NullObjectSftpuploadDecorator))
                _sftpFileUploadDecorator.UploadFile(pathOfFileToUpload, ref destinationFilePath, sftpClient, logger);
        }

        public virtual void PerformPostUploadOperation(string pathOfFileToUpload, ref string destinationFilePath, SftpClient sftpClient, ILogger logger)
        {
            System.Diagnostics.Debug.WriteLine($"Called PerformPostUploadOperation() on {this.GetType().Name}");
            logger.LogDebug($"Called PerformPostUploadOperation() on {this.GetType().Name}");

            if (!(_sftpFileUploadDecorator is NullObjectSftpuploadDecorator))
                _sftpFileUploadDecorator.PerformPostUploadOperation(pathOfFileToUpload, ref destinationFilePath, sftpClient, logger);
        }
    }
}