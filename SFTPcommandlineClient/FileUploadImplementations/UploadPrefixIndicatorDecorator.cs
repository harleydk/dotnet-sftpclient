using System.IO;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace DotNetSftp.FileUploadImplementations
{
    /// <summary>
    /// This add a 'file-upload indicator' prefix so that it's possible to tell if a file is in the processing of being uploaded,
    /// and keeping away from it until it's done (and the indicator mark is removed).
    /// </summary>
    /// <example>
    /// For example, we might want to add a period, '.', in front of the file-to-upload. So by using this decorator, we would rename the file to include
    /// the period, upload it, then rename it back.
    /// </example>
    public class UploadPrefixIndicatorDecorator : SftpFileUploadDecoratorBase
    {
        private readonly string _uploadPrefixIndicator;
        private string _originalDestinationFilePath;
        private string _renamedDestinationFilePath;

        public UploadPrefixIndicatorDecorator(string uploadPrefix, ISftpFileUploadDecorator sftpFileUploadDecorator) : base(sftpFileUploadDecorator)
        {
            _uploadPrefixIndicator = uploadPrefix;
        }

        public override void PerformPreUploadOperation(string pathOfFileToUpload, ref string destinationFilePath, SftpClient sftpClient, ILogger logger)
        {
            base.PerformPreUploadOperation(pathOfFileToUpload, ref destinationFilePath, sftpClient, logger);

            string fileName = Path.GetFileName(pathOfFileToUpload);

            _originalDestinationFilePath = destinationFilePath;
            _renamedDestinationFilePath = destinationFilePath.Replace(fileName, _uploadPrefixIndicator + fileName);

            logger.LogInformation($"Renaming {_originalDestinationFilePath} to {_renamedDestinationFilePath}");
            destinationFilePath = _renamedDestinationFilePath;
        }

        public override void PerformPostUploadOperation(string pathOfFileToUpload, ref string destinationFilePath, SftpClient sftpClient, ILogger logger)
        {
            base.PerformPostUploadOperation(pathOfFileToUpload, ref destinationFilePath, sftpClient, logger);

            logger.LogInformation($"Renaming {_renamedDestinationFilePath} to {_originalDestinationFilePath}");

            if (sftpClient.Exists(_originalDestinationFilePath))
            {
                // if we are uploading a file we have already uploaded once, it will not be possible to rename the temporary file until we delete the first one. 
                sftpClient.DeleteFile(_originalDestinationFilePath);
            }

            sftpClient.RenameFile(_renamedDestinationFilePath, _originalDestinationFilePath);
        }
    }
}