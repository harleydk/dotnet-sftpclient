namespace DotNetSftp.Settings
{
    public class ConnectivitySettings
    {
        public string Host { get; set; }

        public int Port { get; set; } = 22;

        public string UserName { get; set; }

        public string Password { get; set; }

        public string PrivateKeyPath { get; set; }
        
        
    }
}