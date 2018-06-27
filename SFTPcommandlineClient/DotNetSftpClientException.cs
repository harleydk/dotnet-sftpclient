using System;

namespace DotNetSftp
{
    /// <summary>
    /// Exception-abstraction for our application.
    /// </summary>
    public class DotNetSftpClientException : Exception
    {
        public DotNetSftpClientException(string message) : base(message)
        {
        }

        public DotNetSftpClientException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}