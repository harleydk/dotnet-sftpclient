using System;

namespace DotNetSftp.Settings
{
    public class SettingsParsingException : Exception
    {
        public SettingsParsingException(string message) : base(message)
        {
            // deliberately empty constructor.
        }
    }
}