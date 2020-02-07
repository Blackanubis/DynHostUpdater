using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DynHostUpdater.Helpers
{
    /// <summary>
    ///     EncryptionHelper
    /// </summary>
    public class EncryptionHelper
    {
        #region Fields

        /// <summary>
        /// The encryption key
        /// </summary>
        private const string EncryptionKey = "DynHostUpdater840120";

        #endregion

        #region Methods

        /// <summary>
        ///     Encrypts the specified clear text.
        /// </summary>
        /// <param name="clearText">The clear text.</param>
        /// <returns></returns>
        public static string Encrypt(string clearText)
        {
            var cryptText = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(clearText))
                {
                    var clearBytes = Encoding.Unicode.GetBytes(clearText);

                    using var encryptor = Aes.Create();

                    var pdb = new Rfc2898DeriveBytes(EncryptionKey,
                        new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});

                    if (encryptor == null) return clearText;

                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using var ms = new MemoryStream();
                    using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }

                    cryptText = Convert.ToBase64String(ms.ToArray());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return cryptText;
        }

        /// <summary>
        ///     Decrypts the specified cipher text.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <returns></returns>
        public static string Decrypt(string cipherText)
        {
            cipherText = cipherText.Replace(" ", "+");

            var cipherBytes = Convert.FromBase64String(cipherText);

            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(EncryptionKey,
                    new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

                if (encryptor == null) return cipherText;

                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using var ms = new MemoryStream();

                using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }

                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }

            return cipherText;
        }

        #endregion
    }
}