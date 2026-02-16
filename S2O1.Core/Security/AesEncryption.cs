using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace S2O1.Core.Security
{
    public static class AesEncryption
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("S2O1-Warehouse-Management-Sys-Key"); // 32 chars? No, 33. Needs 32 bytes for AES-256.
        // Let's use a fixed 32 byte key for simplicity or hash a passphrase.
        // "S2O1-Secure-License-Key-2026-X" (30 chars).
        // Let's use SHA256 of a phrase to get 32 bytes.
        
        private static byte[] GetKey()
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes("S2O1-Warehouse-Super-Secret-Key"));
        }

        private static byte[] GetIV() 
        {
             // Fixed IV for deterministic encryption? Or random?
             // If random, we need to prepend it.
             // Standard: Prepend IV.
             return new byte[16]; 
        }

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = GetKey();
            aes.GenerateIV();
            var iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key, iv);
            using var ms = new MemoryStream();
            // Write IV first
            ms.Write(iv, 0, iv.Length);
            
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText)
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            aes.Key = GetKey();
            
            var iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(fullCipher, 16, fullCipher.Length - 16);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            
            return sr.ReadToEnd();
        }
    }
}
