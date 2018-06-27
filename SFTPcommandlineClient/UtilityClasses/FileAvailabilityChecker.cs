using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace DotNetSftp.UtilityClasses
{
    /// <summary>
    /// Checks if a file is available and ready to be read.
    /// </summary>
    /// <remarks>
    /// Cannot take credit for this code: thanks, https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use !
    /// </remarks>
    public class FileAvailabilityChecker
    {
        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_LOCK_VIOLATION = 33;

        private bool IsFileLocked(Exception exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION;
        }

        /// <summary>
        /// Checks the availability of a single file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>A FileAvailabilityCheck object, that informs us of the file's availability at the time the function was run.</returns>
        public FileAvailabilityCheck CheckFileAvailability(string filePath)
        {
            try
            {
                //The "using" is important because FileStream implements IDisposable and
                //"using" will avoid a heap exhaustion situation when too many handles
                //are left undisposed.
                using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    FileAvailabilityCheck fileWasAvailable = new FileAvailabilityCheck
                    {
                        FileName = filePath,
                        FileAvailabilityResult = FileAvailabilityResult.IsReadable,
                        Description =
                            $"At {DateTime.Now.ToShortTimeString()}, {Path.GetFileName(filePath)} was available."
                    };
                    return fileWasAvailable;
                }
            }
            catch (IOException ex)
            {
                if (IsFileLocked(ex))
                {
                    FileAvailabilityCheck notAvailableBecauseLocked = new FileAvailabilityCheck
                    {
                        FileName = filePath,
                        FileAvailabilityResult = FileAvailabilityResult.IsLockedOrShared,
                        Description =
                            $"At {DateTime.Now.ToShortTimeString()}, {Path.GetFileName(filePath)} was locked or shared."
                    };
                    return notAvailableBecauseLocked;
                }

                FileAvailabilityCheck notAvailableBecauseOfIoException = new FileAvailabilityCheck
                {
                    FileName = filePath,
                    FileAvailabilityResult = FileAvailabilityResult.IsLockedOrShared,
                    Description =
                        $"At {DateTime.Now.ToShortTimeString()}, {Path.GetFileName(filePath)} was not available. Error: '{ex.Message}'."
                };
                return notAvailableBecauseOfIoException;
            }
            catch (UnauthorizedAccessException authEx)
            {
                FileAvailabilityCheck notavailableBecauseOfAuthenticationProblem = new FileAvailabilityCheck
                {
                    FileName = filePath,
                    FileAvailabilityResult = FileAvailabilityResult.AccessDenied,
                    Description = $"At {DateTime.Now.ToShortTimeString()}, {Path.GetFileName(filePath)} was not available. Error: '{authEx.Message}'."
                };
                return notavailableBecauseOfAuthenticationProblem;
            }

        }
    }
}