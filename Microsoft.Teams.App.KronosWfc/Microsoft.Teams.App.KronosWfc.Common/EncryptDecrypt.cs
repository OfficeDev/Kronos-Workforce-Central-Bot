//-----------------------------------------------------------------------
// <copyright file="SubscribeAllRepository.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Common
{
    using System;
    using System.Configuration;
    using System.Security.Cryptography;
    using System.Text;
    

    /// <summary>
    /// Class is used to encrypt and decrypt incoming value
    /// </summary>
    public class EncryptDecrypt
    {
        // Initialization vector
        static string AesIV256 = AppSettings.Instance.EncryptionIV;

        // Encryption key
        static string AesKey256 = AppSettings.Instance.EncryptionKey;

        /// <summary>
        /// AES encryption
        /// </summary>
        public static string Encrypt256(string text)
        {
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                
                aes.BlockSize = 128;
                aes.KeySize = 256;
                    
                aes.IV = Encoding.UTF8.GetBytes(AesIV256);
                aes.Key = Encoding.UTF8.GetBytes(AesKey256);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                
                // Convert string to byte array
                byte[] src = Encoding.Unicode.GetBytes(text);

                // encryption
                using (ICryptoTransform encrypt = aes.CreateEncryptor())
                {
                    byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);

                    // Convert byte array to Base64 strings
                    return Convert.ToBase64String(dest);
                }
            }
        }

        /// <summary>
        /// AES decryption
        /// </summary>
        public static string Decrypt256(string text)
        {
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.IV = Encoding.UTF8.GetBytes(AesIV256);
                aes.Key = Encoding.UTF8.GetBytes(AesKey256);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Convert Base64 strings to byte array
                byte[] src = System.Convert.FromBase64String(text);

                // decryption
                using (ICryptoTransform decrypt = aes.CreateDecryptor())
                {
                    byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                    return Encoding.Unicode.GetString(dest);
                }
            }
        }
    }
}
