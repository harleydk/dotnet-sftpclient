namespace DotNetSftp.Settings
{
    public class ApplicationSettings
    {
        public bool ShowHelp { get; set; } = false;

        public string DiskLogLocation { get; set; } = null;

        public string SettingsFilePath { get; set; }
        public string SettingsKeyFilePath { get; set; }
    }
}