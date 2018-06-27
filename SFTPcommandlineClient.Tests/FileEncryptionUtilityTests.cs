using System;
using System.IO;
using DotNetSftp.UtilityClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dotnetsftp.UnitTests
{
    [TestClass()]
    public class FileEncryptionUtilityTests
    {
        [TestMethod()]
        public void EncryptFileTest()
        {
            // arrange
            FileEncryptionUtility fileEncryptionUtility = new FileEncryptionUtility();
            string stringToEncrypt = "stringToEncrypt";
            string encryptedFilePath = Path.GetTempPath() + $"{DateTime.Now.Ticks.ToString()}";
            string encryptionKey = Guid.NewGuid().ToString().Substring(0, 8);

            // act
            fileEncryptionUtility.EncryptFile(stringToEncrypt, encryptedFilePath, encryptionKey);

            // assert
            Assert.IsTrue(File.Exists(encryptedFilePath));
        }

        [TestMethod()]
        public void DecryptFileTest()
        { 
            // arrange
            FileEncryptionUtility fileEncryptionUtility = new FileEncryptionUtility();
            string stringToEncrypt = "stringToEncrypt";
            string encryptedFilePath = Path.GetTempPath() + $"{DateTime.Now.Ticks.ToString()}";
            string encryptionKey = Guid.NewGuid().ToString().Substring(0, 8);

            // act
            fileEncryptionUtility.EncryptFile(stringToEncrypt, encryptedFilePath, encryptionKey);
            string decryptedText = fileEncryptionUtility.DecryptFile(encryptedFilePath, encryptionKey);

            // assert
            Assert.AreEqual(decryptedText, "stringToEncrypt");
        }

    }
}