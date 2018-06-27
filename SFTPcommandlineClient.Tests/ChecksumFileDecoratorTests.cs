using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetSftp.FileUploadImplementations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetSftp.FileUploadImplementations.Tests
{
    [TestClass()]
    public class ChecksumFileDecoratorTests
    {
        [TestMethod()]
        public void GetChecksumTest()
        {
            // arrange
            ChecksumFileDecorator checksumFileDecorator = new ChecksumFileDecorator(new NullObjectSftpuploadDecorator());
            var tempFile = Path.GetTempFileName();
            string checkSumText = "The big brown fox ahh screw this";
            File.WriteAllText(tempFile, checkSumText);

            // act
            string checkSum = checksumFileDecorator.GetChecksum(tempFile);

            // assert
            string expectedChecksum = @"6961FA4079D1BCAF245E2BBBA39B39A0251CE248F2B0B6EC2061E07A20C62EF4";
            Assert.AreEqual(checkSum,expectedChecksum);

            // clean up
            File.Delete(tempFile);
        }
    }
}