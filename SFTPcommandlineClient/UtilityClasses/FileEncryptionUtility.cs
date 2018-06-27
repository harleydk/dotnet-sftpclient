using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DotNetSftp.UtilityClasses
{
    /// <summary>
    /// Provides functionality to encrypt/decrypt files. Originally authored by Steve Lydford in his CodeProject-article. Great work, Steve!
    /// </summary>
    /// <see cref="https://www.codeproject.com/Articles/26085/File-Encryption-and-Decryption-in-C"/>
    public class FileEncryptionUtility
    {
        /// <summary>
        ///  Steve Lydford - 12/05/2008.
        /// 
        ///  Encrypts a file using Rijndael algorithm.
        /// </summary>
        /// <param name="stringToEncrypt"></param>
        /// <param name="outputFile"></param>
        /// <param name="encryptionKey"></param>
        public void EncryptFile(string stringToEncrypt, string outputFile, string encryptionKey)
        {
            try
            {
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(encryptionKey);

                string cryptFile = outputFile;
                FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create);

                RijndaelManaged RMCrypto = new RijndaelManaged();
                CryptoStream cs = new CryptoStream(fsCrypt, RMCrypto.CreateEncryptor(key, key), CryptoStreamMode.Write);

                byte[] stringToEncryptAsBytes = Encoding.ASCII.GetBytes(stringToEncrypt);
                MemoryStream fsIn = new MemoryStream(stringToEncryptAsBytes);

                int data;
                while ((data = fsIn.ReadByte()) != -1)
                    cs.WriteByte((byte)data);

                fsIn.Close();
                cs.Close();
                fsCrypt.Close();
            }
            catch( Exception ex)
            {
                throw new CryptographicException($"Could not encrypt string '{stringToEncrypt}'.");
            }
        }

        /// <summary>
        ///  Steve Lydford - 12/05/2008.
        /// 
        ///  Decrypts a file using Rijndael algorithm.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="encryptionKey"></param>
        public string DecryptFile(string inputFile, string encryptionKey)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] key = UE.GetBytes(encryptionKey);

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);

            RijndaelManaged RMCrypto = new RijndaelManaged();

            CryptoStream cs = new CryptoStream(fsCrypt,
                RMCrypto.CreateDecryptor(key, key),
                CryptoStreamMode.Read);

            MemoryStream fsOut = new MemoryStream();
            //FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            int data;
            while ((data = cs.ReadByte()) != -1)
                fsOut.WriteByte((byte)data);

            string settingsString = Encoding.ASCII.GetString(fsOut.ToArray());

            fsOut.Close();
            cs.Close();
            fsCrypt.Close();

            return settingsString;
        }
    }
}
