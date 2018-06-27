namespace DotNetSftp.Settings
{
    public class TransferSettings
    {
        public char TransferType { get; set; }

        public bool OverWriteExistingFiles { get; set; } = false;

        public int NumberOfRetries { get; set; } = 3;

        public string SourcePath { get; set; }

        public string DestinationPath { get; set; }

        public string UploadPrefix { get; set; }

        public bool CalculateChecksum { get; set; } = false;

        public bool CompressDirectoryBeforeUpload { get; set; } = false;
    }
}