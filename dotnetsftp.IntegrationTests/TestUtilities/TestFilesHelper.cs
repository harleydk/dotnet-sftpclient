using System;
using System.IO;

namespace dotnetsftp.IntegrationTests.TestUtilities
{
    public static class TestFilesHelper
    {
        public static string GetTestFileDir()
        {
            DirectoryInfo testDir = new DirectoryInfo(AppContext.BaseDirectory);
            testDir = new DirectoryInfo(testDir + @"\TestFiles\");
            return testDir.FullName + @"\";
        }
    }
}