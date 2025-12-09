using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ConfidoSoft.Data.Services.BLServices
{
    /// <summary>
    /// Generic Data encryption service.
    /// </summary>
    public class DataEncryptionService
    {
        //Fix Application key and IV to be used for some of the records...
        public const string AppKey = "KM8GF27$QNJ59M3147H9A318YBF5S5HM";
        public const string AppIV = "295DMN83JQZWKORV";

        private readonly byte[] _byteKey;
        private readonly byte[] _byteIV;

        /// <summary>
        /// Create new instance of DataEncryptionService which will be used to  encrypt value with
        /// random IV. Useful for which database search is not required 
        /// and where fix key and different iv can be used. e.g. Setting table.
        /// </summary>
        /// <param name="key"> Encryption key to be used.</param>
        public DataEncryptionService(String key)
        {
            this.Key = key;
            this._byteKey = Encoding.UTF8.GetBytes(key);            
        }

        /// <summary>
        /// Create new instance of DataEncryptionService which will be used to  encrypt value with
        /// given key and iv value.. useful for data which need to encrypt decryption
        /// with given key and fixed iv e.g. user personal data.
        /// </summary>
        /// <param name="key"> 32-bit encryption key</param>
        /// <param name="iv">IV to be used</param>
        public DataEncryptionService(String key, String iv)
        {
            this.Key = key;
            this._byteKey = Encoding.UTF8.GetBytes(key);
            this._byteIV = Encoding.UTF8.GetBytes(iv);         
        }

        /// <summary>
        /// REturn currently used encryption key
        /// </summary>
        public String Key { get; private set; }



        /// <summary>
        /// Encrypt given input message, and return encrypted string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Encrypt(string text)
        {
            if (String.IsNullOrWhiteSpace(text) == false)
            {
                if (this._byteIV != null)
                {
                    return Encrypt(text, this._byteKey, this._byteIV);
                }
                else
                {
                    return EncryptWithIV(text, this._byteKey);
                }
            }
            else
            {
                return text;
            }
        }

        /// <summary>
        /// Decrypt given input message, and return decrypted string.
        /// </summary>
        /// <param name="cipherText">encrypted string/data</param>
        /// <returns> decrypted string/data</returns>
        public string Decrypt(string cipherText)
        {
            if (String.IsNullOrWhiteSpace(cipherText) == false)
            {
                if (this._byteIV != null)
                {
                    return Decrypt(cipherText, this._byteKey, this._byteIV);
                }
                else
                {
                    return DecryptWithIV(cipherText, _byteKey);
                }
            }
            else
            {
                return cipherText;
            }
        }

        #region helper function

        /// <summary>
        /// Encrypt string with given key and iv.
        /// </summary>
        /// <param name="text"> text to be encrypted</param>
        /// <param name="key"> key to be used</param>
        /// <param name="iv">iv to be used</param>
        /// <returns></returns>
        private static string Encrypt(string text, byte[] key, byte[] iv)
        {
            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, iv))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(text);
                            }
                        }
                        var encryptedContent = msEncrypt.ToArray();
                        return Convert.ToBase64String(encryptedContent);
                    }
                }
            }
        }

        /// <summary>
        /// Decrypt the string. using given key and iv
        /// </summary>
        /// <param name="cipherText">encrypted string</param>
        /// <param name="key">key to be used.</param>
        /// <param name="iv">iv to be used.</param>
        /// <returns></returns>
        private static string Decrypt(string cipherText, byte[] key, byte[] iv)
        {
            string result = null;
            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    var fullCipher = Convert.FromBase64String(cipherText);
                    //var cipher = new byte[fullCipher.Length];                
                    using (var msDecrypt = new MemoryStream(fullCipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Encrypt with random IV which is included in encrypted value.
        /// </summary>
        /// <param name="text"> text to be encrypted</param>
        /// <param name="key">key to be used.</param>
        /// <returns></returns>
        //http://mikaelkoskinen.net/post/encrypt-decrypt-string-asp-net-core
        private static string EncryptWithIV(string text, byte[] key)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.GenerateIV();
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(text);
                            }
                        }

                        var iv = aesAlg.IV;

                        var encryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + encryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        /// <summary>
        /// Decrypt value by using IV in encrypted text.
        /// </summary>
        /// <param name="cipherText">encrypted text with IV</param>
        /// <param name="key">key to be used.</param>
        /// <returns></returns>
        private static string DecryptWithIV(string cipherText, byte[] key)
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            var iv = new byte[16];
            var cipherLen = fullCipher.Length - 16;
            var cipher = new byte[cipherLen];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);
            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                    return result;
                }
            }
        }
        
        #endregion

    }
}
