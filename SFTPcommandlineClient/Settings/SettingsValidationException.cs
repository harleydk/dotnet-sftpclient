using System;

namespace DotNetSftp.Settings
{
    public class SettingsValidationException : Exception
    {
        public SettingsValidationException(string message) : base(message)
        {
            // deliberately empty constructor.
        }
    }
}