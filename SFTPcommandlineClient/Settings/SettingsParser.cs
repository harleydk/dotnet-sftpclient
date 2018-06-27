using System;
using System.IO;
using System.Linq;
using Mono.Options;
using Newtonsoft.Json;

namespace DotNetSftp.Settings
{
    public class SettingsParser
    {
        [JsonIgnore]
        public OptionSet Options;

        public TransferSettings TransferSettings;
        public ConnectivitySettings ConnectivitySettings;
        public ApplicationSettings ApplicationSettings;

        public SettingsParser()
        {
            TransferSettings = new TransferSettings();
            ConnectivitySettings = new ConnectivitySettings();
            ApplicationSettings = new ApplicationSettings();
        }

        public void ParseSettingsFromCommandlineArguments(string[] args)
        {
            try
            {
                // Options to be read from the command-line:
                Options = new OptionSet()
                    {
                        { "tt=|transferType=", "The transfer-type we would like to invoke. Specify 'u|upload' for upload, 'd|download' for download.", option => TransferSettings.TransferType = option.ToLower()[0] },
                        { "host=", "The address of the host sftp server", option => ConnectivitySettings.Host = option },
                        { "port=", "The port-number of the host sftp server (defaults to 22).", (int option ) => ConnectivitySettings.Port = option  },
                        { "u=|username=", "The user-name of the sftp-account user.", option => ConnectivitySettings.UserName = option },
                        { "p=|password=", "The password of the sftp-account user. Doubles as pass-phrase, if connecting via a private key.", option => ConnectivitySettings.Password = option },
                        { "sp=|sourcePath=", "The valid, full path of the source. May be a file or an entire directory.", option => TransferSettings.SourcePath = option },
                        { "dp=|destinationPath=", "The valid, full destionation path. For upload operations, seperate directories with '/', ex. 'dp=/home/user'. Non-existing directories will be created.", option => TransferSettings.DestinationPath = option },
                        { "ow|overWriteExisting", "Should overwrite any existing file. If set to false, any existing files will simply be skipped.", option => TransferSettings.OverWriteExistingFiles = option != null },
                        { "nr=|numberOfRetries=", "The number of transfer-retries in case of connectity issues (defaults to 3).", (int option ) => TransferSettings.NumberOfRetries = option  },
                        { "pk=|privateKeyPath", "The full path to the private key, used to authenticate against the server's public key.", option => ConnectivitySettings.PrivateKeyPath = option },
                        { "h|help", "Show this message and exit.", option => ApplicationSettings.ShowHelp = option != null },
                        { "log=", "The path to the directory where log-information should be written. Defaults to the app's install-dir.", option => ApplicationSettings.DiskLogLocation = option },
                        { "cs|checkSum", "Calculate a SHA256 checksum for the files and upload it, but with '.sha256' extension.", option => TransferSettings.CalculateChecksum = option != null },
                        { "cd|compressDirectory", "Compresses the source-dir into a Zip-file before uploading it.", option => TransferSettings.CompressDirectoryBeforeUpload = option != null },
                        { "up=|uploadPrefix=", "Add a prefix in front of the files while uploading, to be removed when the upload has completed.", option => TransferSettings.UploadPrefix = option  },
                        { "sf=|settingsFilePath=", "If specified, a settings-file will be generated at the specified location, containing all other arguments. " +
                                                   "This settings file can then be used to simplify transfers. " +
                                                   "If a settings-file already exists at the specified path, a transfer will be executed from the settings.", option => ApplicationSettings.SettingsFilePath = option },
                        { "sfKey=|settingsKeyFilePath=", "If specified, and a settings-file is also specified and exists, the settings-file will be decrypted using a key in this key-file before being executed. " +
                            "If specified, and a settings-file is also specified but does not exists, a key will be automatically generated and saved to this key-file.", option => ApplicationSettings.SettingsKeyFilePath = option },
                   };

                Options.Parse(args);
            }
            catch (OptionException e)
            {
                throw new SettingsParsingException(e.Message);
            }
        }

        /// <summary>
        /// Basic validation of transfersettings - the file exists, the directory exists, so on and so forth.
        /// </summary>
        public void ValidateTransferSettings()
        {
            // validate transfer-type
            if (!new char[] { 'u', 'U', 'd', 'D' }.Contains(TransferSettings.TransferType))
                throw new SettingsValidationException($"Invalid transfer-type specified, was '{TransferSettings.TransferType}, expected 'u|upload|d|download'");

            // check local paths.
            char transferTypeChar = TransferSettings.TransferType;
            if (transferTypeChar == 'u' || transferTypeChar == 'U')
            {
                // Uploading, check that the local source exists
                ThrowValidationErrorIfFileSystemEntryDoesntExist(TransferSettings.SourcePath);
            }
            else if (transferTypeChar == 'd' || transferTypeChar == 'D')
            {
                // Downloading, check that the local destination drive exists. We only check the root, because we'll automatically be creating the destination directory structure during the download.
                var drivePath = Path.GetPathRoot(TransferSettings.DestinationPath);
                ThrowValidationErrorIfFileSystemEntryDoesntExist(drivePath);
            }
            else
                throw new ArgumentException("Invalid transfer-type");


            // ensure destination-path does not include backward-slashes
            bool isUpload = TransferSettings.TransferType == 'u' || TransferSettings.TransferType == 'U';
            if (isUpload && TransferSettings.DestinationPath.Contains(@"\"))
            {
                throw new SettingsValidationException($"The destination path '{TransferSettings.DestinationPath}' should not include traditional filesystem backward-slashes - use forward-slashes when uploading to an sft-server.");
            }
        }

        /// <summary>
        /// Basic validation of application-settings
        /// </summary>
        public void ValidateApplicationSettings()
        {
            // validate settings-file drive exists.
            if (!string.IsNullOrWhiteSpace(ApplicationSettings.SettingsFilePath))
            {
                var drivePath = Path.GetPathRoot(ApplicationSettings.SettingsFilePath);
                ThrowValidationErrorIfFileSystemEntryDoesntExist(drivePath);
            }

            // validate settings-key is not provided without also settings file path
            if (!string.IsNullOrEmpty(ApplicationSettings.SettingsKeyFilePath) && string.IsNullOrWhiteSpace( ApplicationSettings.SettingsFilePath))
            {
                throw new SettingsValidationException($"A settings encryption - key is provided, but a settings - file path is not.");
            }
    }

    private void ThrowValidationErrorIfFileSystemEntryDoesntExist(string fileSystemEntryPath)
    {
        if (string.IsNullOrWhiteSpace(fileSystemEntryPath))
            throw new SettingsValidationException($"The file-system path '{fileSystemEntryPath}' is null or empty.");

        FileAttributes fileSystemEntryAttributes = File.GetAttributes(fileSystemEntryPath);
        if ((fileSystemEntryAttributes & FileAttributes.Directory) == FileAttributes.Directory)
        {
            if (!Directory.Exists(fileSystemEntryPath))
                throw new SettingsValidationException($"The path, '{fileSystemEntryPath}' does not exist.");
        }
        else // is file
        {
            if (!File.Exists(fileSystemEntryPath))
                throw new SettingsValidationException($"The path, '{fileSystemEntryPath}' does not exist.");
        }
    }

}
}