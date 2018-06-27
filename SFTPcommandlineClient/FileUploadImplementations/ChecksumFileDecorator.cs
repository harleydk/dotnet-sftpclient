using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace DotNetSftp.FileUploadImplementations
{
    /// <summary>
    /// Calculates a sha256-bash for the file that will be uploaded, and uploads it into a 'signature'-file, alongside the file that will be uploaded.
    /// </summary>
    public class ChecksumFileDecorator : SftpFileUploadDecoratorBase
    {
        private string _checksum;
        private const string CheckSumFileExtention = @".sha256";

        public ChecksumFileDecorator(ISftpFileUploadDecorator sftpFileUploadDecorator) : base(sftpFileUploadDecorator)
        {
        }

        public override void PerformPreUploadOperation(string pathOfFileToUpload, ref string destinationFilePath, SftpClient sftpClient,
            ILogger logger)
        {
            base.PerformPreUploadOperation(pathOfFileToUpload, ref destinationFilePath, sftpClient, logger);

            // calculate hash for file
            _checksum = GetChecksum(pathOfFileToUpload);
        }

        public string GetChecksum(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        public override void PerformPostUploadOperation(string pathOfFileToUpload, ref string destinationFilePath, SftpClient sftpClient, ILogger logger)
        {
            base.PerformPostUploadOperation(pathOfFileToUpload, ref destinationFilePath, sftpClient, logger);

            // upload sha-file
            string signatureFileUploadName = Path.GetFileName(pathOfFileToUpload) + CheckSumFileExtention;
            MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(_checksum));
            using (ms)
            {
                sftpClient.BufferSize = 4 * 1024;
                sftpClient.UploadFile(ms, signatureFileUploadName, true);
            }
        }
    }
}