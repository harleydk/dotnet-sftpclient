namespace DotNetSftp.UtilityClasses
{
    public enum FileAvailabilityResult
    {
        IsReadable = 0,
        IsNotReadable = 1,
        IsLockedOrShared = 2,
        AccessDenied = 3
    }
}