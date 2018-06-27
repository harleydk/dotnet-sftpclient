using DotNetSftp.FileUploadImplementations;
using DotNetSftp.Settings;
using DotNetSftp.UtilityClasses;
using Microsoft.Extensions.Logging;
using Polly;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DotNetSftp
{
    public class DotNetSftpClient : IDisposable
    {
        private const int DEFAULT_NUMBER_OF_CONNECTION_RETRIES = 3;
        private const int DEFAULT_WAIT_SECONDS = 2;

        private Renci.SshNet.SftpClient _sftpClient;
        private readonly ConnectivitySettings _connectivitySettings;
        private readonly FileAvailabilityChecker _fileAvailabilityChecker;

        private readonly ILogger _logger;

        public DotNetSftpClient(ConnectivitySettings connectivitySettings, ILogger logger)
        {
            this._connectivitySettings = connectivitySettings;
            _fileAvailabilityChecker = new FileAvailabilityChecker();
            _logger = logger;
        }

        public void CreateConnection()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_connectivitySettings.PrivateKeyPath))
                {
                    #region guard clause

                    if (string.IsNullOrWhiteSpace(_connectivitySettings.Host) ||
                        string.IsNullOrWhiteSpace(_connectivitySettings.UserName) ||
                        string.IsNullOrWhiteSpace(_connectivitySettings.Password))
                    {
                        Exception connectivityException = new Exception($"One or more connectivity-settings are incorrect: Host={_connectivitySettings.Host}, UserName={_connectivitySettings.UserName}, password=*******");
                        _logger.LogError(connectivityException, connectivityException.Message);
                        throw connectivityException;
                    }

                    #endregion guard clause

                    // connect using just username/password:
                    _sftpClient = new Renci.SshNet.SftpClient(
                        _connectivitySettings.Host,
                        _connectivitySettings.Port,
                        _connectivitySettings.UserName,
                        _connectivitySettings.Password);
                }
                else
                {
                    #region guard clause

                    if (string.IsNullOrWhiteSpace(_connectivitySettings.Host) ||
                        string.IsNullOrWhiteSpace(_connectivitySettings.UserName) ||
                        String.IsNullOrWhiteSpace(_connectivitySettings.Password) ||
                        string.IsNullOrWhiteSpace(_connectivitySettings.PrivateKeyPath))

                    {
                        Exception connectivityException = new Exception($"One or more connectivity-settings are incorrect: Host={_connectivitySettings.Host}, UserName={_connectivitySettings.UserName}, privateKeyPath={_connectivitySettings.PrivateKeyPath}");
                        _logger.LogError(connectivityException, connectivityException.Message);
                        throw connectivityException;
                    }

                    #endregion guard clause

                    // connect using just username/private key file/password as pass-phrase:
                    _logger.LogInformation($"Will log on with keyfile {_connectivitySettings.PrivateKeyPath}, with pass-phrase '{@_connectivitySettings.Password}'");

                    VerifyPrivateKeyFileIsReadable(_connectivitySettings.PrivateKeyPath);
                    VerifyPrivateKeyIsNotPuttyFormat(_connectivitySettings.PrivateKeyPath);

                    PrivateKeyFile keyFile = new PrivateKeyFile(File.OpenRead(_connectivitySettings.PrivateKeyPath),
                        @_connectivitySettings.Password);

                    _sftpClient = new Renci.SshNet.SftpClient(
                        _connectivitySettings.Host,
                        _connectivitySettings.Port,
                        _connectivitySettings.UserName,
                        keyFile);
                }

                _logger.LogInformation($"Will connect to {_connectivitySettings.Host}, port {_connectivitySettings.Port}...");

                // Retry, waiting a specified duration between each retry
                var policy = Policy
                    .Handle<SocketException>()
                    .WaitAndRetry(DEFAULT_NUMBER_OF_CONNECTION_RETRIES,
                        retryNumber => TimeSpan.FromSeconds(retryNumber * DEFAULT_WAIT_SECONDS),
                    (ex, tsp) =>
                       {
                           // method to call on retries.
                           // TODO: implement logging here.
                           System.Diagnostics.Debug.WriteLine($"Trying again in {tsp}");
                       });
                policy.Execute(() => _sftpClient.Connect());
            }
            catch (Exception exception) when (exception.Message == @"Invalid private key file.") // Catch specific key file error, redirect to possible solution.
            {
                // The SSH.Net component supports RSA and DSA private key in both OpenSSH and SSH.COM format.
                // Some tools, such as PuTTYgen, generates key-files that need to be converted to another format in order to work with SSH.Net.

                Exception innerException = new Exception($@"Invalid key file, possible https://stackoverflow.com/questions/43915130/does-ssh-net-accept-only-openssh-format-of-private-key-if-not-what-are-the-res issue.", exception);
                // wrap exception in own specific exception.
                DotNetSftpClientException dotNetSftpClientException =
                    new DotNetSftpClientException("DotNetSftpClient error, see inner Exception for details", innerException);

                throw dotNetSftpClientException;
            }
            catch (Exception exception)
            {
                // wrap exception in own specific exception.
                DotNetSftpClientException dotNetSftpClientException =
                    new DotNetSftpClientException("DotNetSftpClient error, see inner Exception for details", exception);

                throw dotNetSftpClientException;
            }
        }

        private void VerifyPrivateKeyIsNotPuttyFormat(string connectivitySettingsPrivateKeyPath)
        {
            string allPrivateKeyFileContent = File.ReadAllText(connectivitySettingsPrivateKeyPath);
            if (allPrivateKeyFileContent.Contains(@"PuTTY-User-Key-File"))
                throw new DotNetSftpClientException("Private key file is in PuTTY-User-Key-File format - this is not supported. The private key file must be RSA or DSA private key, in either OpenSSH or ssh.com format.");
        }

        /// <summary>
        /// Verify that the private key file actually exists, and attempt to read it through, to ensure it's accessible.
        /// </summary>
        /// <param name="privateKeyPath">Full path to the private key file.</param>
        private void VerifyPrivateKeyFileIsReadable(string privateKeyPath)
        {
            if (!File.Exists(privateKeyPath))
                throw new System.IO.FileNotFoundException();

            File.ReadAllLines(privateKeyPath);
        }

        public void DisconnectClient()
        {
            _sftpClient.Disconnect();
        }

        public void Upload(TransferSettings transferSettings)
        {
            #region guard clause

            if (String.IsNullOrWhiteSpace(transferSettings.DestinationPath))
            {
                if (string.IsNullOrWhiteSpace(_connectivitySettings.UserName))
                    throw new NotImplementedException("Could not form a destination path based on the user's name - it is blank");

                _logger.LogInformation($"Blank destination-path, will rename to user's home dir");
                transferSettings.DestinationPath = $@"/home/{_connectivitySettings.UserName}";
            }

            #endregion guard clause

            _logger.LogInformation($"Will upload into directory '{transferSettings.DestinationPath}'");
            CreateServerDirectoryIfItDoesntExist(transferSettings.DestinationPath);

            // Create uploader-implementation
            ISftpFileUploader fileUploaderImplementation = CreateSftpUploaderFromTransferSettings(transferSettings, _logger);

            //detect whether its a directory or file, and act accordingly
            FileAttributes fileSystemEntryAttributes = File.GetAttributes(transferSettings.SourcePath);
            if ((fileSystemEntryAttributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                bool shouldZipCompressDirectoryBeforeUpload = transferSettings.CompressDirectoryBeforeUpload;
                if (shouldZipCompressDirectoryBeforeUpload)
                {
                    string tempPath = Path.GetTempPath();
                    DirectoryInfo dirInfo = new DirectoryInfo(transferSettings.SourcePath);
                    string compressedZipFileFullPath = tempPath + @"\" + dirInfo.Name + ".zip";
                    if (File.Exists(compressedZipFileFullPath))
                        File.Delete(compressedZipFileFullPath);

                    ZipFile.CreateFromDirectory(transferSettings.SourcePath, compressedZipFileFullPath);

                    UploadSingleFile(fileUploaderImplementation, compressedZipFileFullPath, transferSettings.DestinationPath, transferSettings.OverWriteExistingFiles);
                }
                else
                {
                    UploadDirectory(transferSettings, fileUploaderImplementation);
                }
            }
            else // is file
            {
                string pathOfFileToUpload = transferSettings.SourcePath;
                UploadSingleFile(fileUploaderImplementation, pathOfFileToUpload, transferSettings.DestinationPath, transferSettings.OverWriteExistingFiles);
            }
        }

        /// <summary>
        /// Will upload an entire directory of files and, recursively, directories within.
        /// </summary>
        public void UploadDirectory(TransferSettings transferSettings, ISftpFileUploader fileUploaderImplementation)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Uploading directory {transferSettings.SourcePath}");
                _logger.LogInformation($"Uploading directory {transferSettings.SourcePath}");

                // upload files within directory
                IEnumerable<string> filesToUpload = Directory.EnumerateFiles(transferSettings.SourcePath, "*.*").ToList();

                // upload all individual files into the server directory
                int uploadedFileCounter = 1;
                foreach (string fileToUpload in filesToUpload)
                {
                    UploadSingleFile(fileUploaderImplementation, fileToUpload, transferSettings.DestinationPath, transferSettings.OverWriteExistingFiles);
                    uploadedFileCounter++;
                }

                // Traverse any sub-directories
                DirectoryInfo dirInfo = new DirectoryInfo(transferSettings.SourcePath);
                DirectoryInfo[] subDirectories = dirInfo.GetDirectories();
                if (!subDirectories.Any())
                    return;

                foreach (DirectoryInfo subDir in subDirectories)
                {
                    // Shallow copy transfer-settings object ...
                    TransferSettings copiedTransferSettings = new TransferSettings
                    {
                        OverWriteExistingFiles = transferSettings.OverWriteExistingFiles,
                        SourcePath = subDir.FullName,
                        DestinationPath = transferSettings.DestinationPath + @"/" + subDir.Name
                    };
                    _logger.LogInformation($"Will upload into directory '{subDir.Name}'");

                    // ... then call UploadDirectory() recursively.
                    CreateServerDirectoryIfItDoesntExist(copiedTransferSettings.DestinationPath);
                    UploadDirectory(copiedTransferSettings, fileUploaderImplementation);
                }
            }
            catch (Exception e)
            {
                throw new DotNetSftpClientException(e.Message, e);
            }
        }

        private void UploadSingleFile(ISftpFileUploader fileUploaderImplementation, string fileToUpload, string destinationPath, bool overWriteExistingFiles)
        {
            System.Diagnostics.Debug.WriteLine($"Will upload {fileToUpload}");

            // check if file can be read
            var fileAvailability = _fileAvailabilityChecker.CheckFileAvailability(fileToUpload);
            if (fileAvailability.FileAvailabilityResult != FileAvailabilityResult.IsReadable)
            {
                _logger.LogError($"File '{fileAvailability.FileName}' could not be read - {fileAvailability.Description}/{fileAvailability.FileAvailabilityResult}");
                return;
            }

            // Remove any trailing forward-slashes.
            if (destinationPath.EndsWith("/"))
                destinationPath = destinationPath.Substring(0, destinationPath.Length - 1);

            string sftpServerDestinationFilePath = destinationPath + @"/" + Path.GetFileName(fileToUpload);
            if (_sftpClient.Exists(sftpServerDestinationFilePath) && overWriteExistingFiles == false)
            {
                string fileNotUploadedMessage = ($"'{fileToUpload}' not uploaded - already exists in destination-dir '{destinationPath}'");
                _logger.LogInformation(fileNotUploadedMessage);
            }
            else if (_sftpClient.Exists(sftpServerDestinationFilePath) && overWriteExistingFiles == true)
            {
                // Exists, overwrite if size changed or newer.
                long existingFileSize = _sftpClient.GetAttributes(sftpServerDestinationFilePath).Size;
                long localFileSize = new FileInfo(fileToUpload).Length;
                bool isDifferentSize = existingFileSize != localFileSize;

                DateTime existingFileTimeStamp = _sftpClient.GetLastWriteTimeUtc(sftpServerDestinationFilePath);
                DateTime localFileTimeStamp = File.GetLastWriteTimeUtc(fileToUpload);
                bool localFileIsNewer = localFileTimeStamp > existingFileTimeStamp;

                if (isDifferentSize || localFileIsNewer)
                {
                    fileUploaderImplementation.PerformFileUpload(fileToUpload, sftpServerDestinationFilePath, _sftpClient, _logger);
                    string fileUploadMessage = ($"Uploaded '{fileToUpload}' into destination-dir '{destinationPath}'");
                    _logger.LogInformation(fileUploadMessage);

                }
                else
                {
                    string fileNotUploadedMessage = ($"'{fileToUpload}' not uploaded - already exists in destination-dir '{destinationPath}'");
                    _logger.LogInformation(fileNotUploadedMessage);
                }
            }
            else
            {
                // All-new file, just upload it.
                fileUploaderImplementation.PerformFileUpload(fileToUpload, sftpServerDestinationFilePath, _sftpClient, _logger);
                string fileUploadMessage = ($"Uploaded '{fileToUpload}' into destination-dir '{destinationPath}'");
                _logger.LogInformation(fileUploadMessage);
            }
        }

        private ISftpFileUploader CreateSftpUploaderFromTransferSettings(TransferSettings transferSettings, ILogger logger)
        {
            // TODO: add decorators based on transfersettings
            ISftpFileUploadDecorator uploader = new BasicSftpFileUploaderDecorator(new NullObjectSftpuploadDecorator());

            if (!string.IsNullOrWhiteSpace(transferSettings.UploadPrefix))
                uploader = new UploadPrefixIndicatorDecorator(transferSettings.UploadPrefix, uploader);

            if (transferSettings.CalculateChecksum == true)
                uploader = new ChecksumFileDecorator(uploader);

            //uploader = new ChecksumFileDecorator(uploader);

            ISftpFileUploader standardFileUploader = new StandardSftpFileUploader(uploader, logger);
            return standardFileUploader;
        }

        private void CreateServerDirectoryIfItDoesntExist(string serverDestinationPath)
        {
            bool serverDirectoryExists = _sftpClient.Exists(serverDestinationPath);
            if (serverDirectoryExists)
            {
                _logger.LogInformation($"The server directory '{serverDestinationPath}' already exists, will upload into it.");
            }
            else
            {
                _logger.LogInformation($"The server directory '{serverDestinationPath}' will be created.");
                string[] directories = serverDestinationPath.Split('/');
                for (int i = 0; i < directories.Length; i++)
                {
                    string dirName = string.Join("/", directories, 0, i + 1);
                    if (!string.IsNullOrWhiteSpace(dirName) && !_sftpClient.Exists(dirName))
                    {
                        System.Diagnostics.Debug.WriteLine($"Creating dir {dirName} on the server.");
                        _sftpClient.CreateDirectory(dirName);
                    }
                }
            }
        }

        public void Dispose()
        {
            _sftpClient.Disconnect();
        }

        public void Download(TransferSettings settingsParserTransferSettings)
        {
            throw new NotImplementedException();
        }
    }
}