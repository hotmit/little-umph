using System;
using System.IO;
using System.Security.Cryptography;
#if NET35_OR_GREATER
using System.Linq;
#endif
using System.Text;

namespace LittleUmph
{
    /// <summary>
    /// Simple AES encryption.
    /// </summary>
    public class SimpleEncryption
    {
        #region [ AES Encryption ]
        /// <summary>
        /// Encrypts the specified clear text (return Base64 encoded string).
        /// </summary>
        /// <param name="clearText">The clear text.</param>
        /// <param name="passphrase">The pass phrase, this can be anything and any length (MUST use this same pass phrase to decrypt).</param>
        /// <returns>Base64 encoded string.</returns>
        public static string Encrypt(string clearText, string passphrase)
        {
            byte[] key;
            byte[] vector;
            createKeyAndInitVector(passphrase, out key, out vector);

            RijndaelManaged rm = new RijndaelManaged();
            ICryptoTransform encryptorTransform = rm.CreateEncryptor(key, vector);
            Byte[] bytes = Encoding.UTF8.GetBytes(clearText);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(memoryStream, encryptorTransform, CryptoStreamMode.Write))
                {
                    cs.Write(bytes, 0, bytes.Length);
                    cs.FlushFinalBlock();

                    memoryStream.Position = 0;
                    byte[] encrypted = new byte[memoryStream.Length];
                    memoryStream.Read(encrypted, 0, encrypted.Length);

                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        /// <summary>
        /// Decrypts the specified base64 encoded cypher.
        /// </summary>
        /// <param name="base64Cypher">The cypher text in base64 encoded format.</param>
        /// <param name="passphrase">The pass phrase (use the same pass phrase when encrypted).</param>
        /// <returns></returns>
        public static string Decrypt(string base64Cypher, string passphrase)
        {
            if (Str.IsEmpty(base64Cypher))
            {
                return "";
            }

            byte[] key;
            byte[] vector;
            createKeyAndInitVector(passphrase, out key, out vector);

            RijndaelManaged rm = new RijndaelManaged();
            ICryptoTransform decryptorTransform = rm.CreateDecryptor(key, vector);
            Byte[] bytes = Convert.FromBase64String(base64Cypher);

            using (MemoryStream encryptedStream = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(encryptedStream, decryptorTransform, CryptoStreamMode.Write))
                {
                    cs.Write(bytes, 0, bytes.Length);
                    cs.FlushFinalBlock();

                    encryptedStream.Position = 0;
                    Byte[] decryptedBytes = new Byte[encryptedStream.Length];
                    encryptedStream.Read(decryptedBytes, 0, decryptedBytes.Length);

                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }

        /// <summary>
        /// Creates the key and vector from the pass phrase.
        /// </summary>
        /// <param name="passphrase">The pass phrase.</param>
        /// <param name="key">The key (32 bytes).</param>
        /// <param name="vector">The vector (16 bytes).</param>
        private static void createKeyAndInitVector(string passphrase, out byte[] key, out byte[] vector)
        {
            // 32 bytes (256bits)
            const string STR_SecretSeed = "jr9Q6K1FAz"; // not so secret now is it? :)

            string keyMd5 = MD5Hash(MD5Hash(passphrase) + passphrase + STR_SecretSeed);
            // 16 bytes (128bits)
            string vectorMd5 = MD5Hash(keyMd5 + passphrase).Substring(passphrase.Length % 16, 16);

            key = Encoding.UTF8.GetBytes(keyMd5);
            vector = Encoding.UTF8.GetBytes(vectorMd5);
        }
        #endregion

        #region [ File & Folder AES Encryption ]
        /// <summary>
        /// Encrypts the dir.
        /// </summary>
        /// <param name="sourceFile">The source dir.</param>
        /// <param name="outputFile">The output dir.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns></returns>
        public static bool EncryptFile(string sourceFile, string outputFile, string passphrase)
        {
            try
            {
                byte[] key;
                byte[] vector;
                createKeyAndInitVector(passphrase, out key, out vector);

                RijndaelManaged rm = new RijndaelManaged();
                ICryptoTransform encryptorTransform = rm.CreateEncryptor(key, vector);

                using (var fsSrc = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                {
                    using (var fsOutput = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                    {
                        using (CryptoStream cs = new CryptoStream(fsOutput, encryptorTransform, CryptoStreamMode.Write))
                        {
                            byte[] buffer = new byte[4096];

                            int count = 0;
                            while ((count = fsSrc.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                cs.Write(buffer, 0, count);
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Decrypts the dir.
        /// </summary>
        /// <param name="sourceFile">The source dir.</param>
        /// <param name="outputFile">The output dir.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns></returns>
        public static bool DecryptFile(string sourceFile, string outputFile, string passphrase)
        {
            try
            {
                byte[] key;
                byte[] vector;
                createKeyAndInitVector(passphrase, out key, out vector);

                RijndaelManaged rm = new RijndaelManaged();
                ICryptoTransform decryptorTransform = rm.CreateDecryptor(key, vector);

                using (var fsSrc = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                {
                    using (var fsOutput = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                    {
                        using (CryptoStream cs = new CryptoStream(fsOutput, decryptorTransform, CryptoStreamMode.Write))
                        {
                            byte[] buffer = new byte[4096];

                            int count = 0;
                            while ((count = fsSrc.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                cs.Write(buffer, 0, count);
                            }
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encrypts the folder.
        /// </summary>
        /// <param name="sourceFolder">The source folder.</param>
        /// <param name="outputFolder">The output folder.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns></returns>
        public static bool EncryptFolder(DirectoryInfo sourceFolder, DirectoryInfo outputFolder, string passphrase)
        {
            return encryptFolder(sourceFolder, outputFolder, "", passphrase);
        }

        /// <summary>
        /// Encrypts the folder.
        /// </summary>
        /// <param name="sourceFolder">The source folder.</param>
        /// <param name="outputFolder">The output folder.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns></returns>
        private static bool encryptFolder(DirectoryInfo sourceFolder, DirectoryInfo outputFolder, string relativePath, string passphrase)
        {
            try
            {
                foreach (var dir in sourceFolder.GetDirectories())
                {
                    encryptFolder(dir, outputFolder, Path.Combine(relativePath, dir.Name), passphrase);
                }

                string folder = Path.Combine(outputFolder.FullName, relativePath);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                foreach (var file in sourceFolder.GetFiles())
                {
                    EncryptFile(file.FullName, Path.Combine(folder, file.Name + ".encrypted"), passphrase);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Decrypts the folder.
        /// </summary>
        /// <param name="sourceFolder">The source folder.</param>
        /// <param name="outputFolder">The output folder.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns></returns>
        public static bool DecryptFolder(DirectoryInfo sourceFolder, DirectoryInfo outputFolder, string passphrase)
        {
            return decryptFolder(sourceFolder, outputFolder, "", passphrase);
        }

        /// <summary>
        /// Decrypts the folder.
        /// </summary>
        /// <param name="sourceFolder">The source folder.</param>
        /// <param name="outputFolder">The output folder.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns></returns>
        private static bool decryptFolder(DirectoryInfo sourceFolder, DirectoryInfo outputFolder, string relativePath, string passphrase)
        {
            try
            {
                foreach (var dir in sourceFolder.GetDirectories())
                {
                    decryptFolder(dir, outputFolder, Path.Combine(relativePath, dir.Name), passphrase);
                }

                string folder = Path.Combine(outputFolder.FullName, relativePath);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                foreach (FileInfo file in sourceFolder.GetFiles("*.encrypted"))
                {
                    string outputFile = Path.Combine(folder, Path.GetFileNameWithoutExtension(file.FullName));
                    SimpleEncryption.DecryptFile(file.FullName, outputFile, passphrase);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region [ MD5 ]
        /// <summary>
        /// Creates the MD5 hash code from the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string MD5Hash(string text)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hash = Encoding.UTF8.GetBytes(text);
            hash = md5.ComputeHash(hash);
            return ByteArr.ToHex(hash);
        }
        #endregion
    }
}
