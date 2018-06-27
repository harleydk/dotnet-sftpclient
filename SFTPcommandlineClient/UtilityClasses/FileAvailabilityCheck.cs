namespace DotNetSftp.UtilityClasses
{
    public struct FileAvailabilityCheck
    {
        public string FileName { get; set; }

        public FileAvailabilityResult FileAvailabilityResult { get; set; }

        public string Description { get; set; }
    }
}