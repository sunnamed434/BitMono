using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BitMono.Encryption
{
    public static class Encryptor
    {
        private static byte[] saltBytes = new byte[]
        {
            101, 65, 10, 102, 205, 97, 73, 24, 12, 3, 4, 5, 6, 7, 10, 19, 26, 83, 73, 45, 119, 11, 37, 38, 39, 50, 40, 41,
        };
        private static byte[] cryptKeyBytes = new byte[]
        {
            70, 101, 20, 10, 37, 10, 47, 17, 14, 15, 13, 12, 11, 10, 59, 95, 66, 75, 99, 88, 77, 66, 44, 55, 10, 85, 90, 101, 110, 30, 40, 50, 51, 60,
        };


        internal static string Decrypt(byte[] bytes)
        {
            byte[] decryptedBytes = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(cryptKeyBytes, saltBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Mode = CipherMode.CBC;

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(bytes, 0, bytes.Length);
                        cryptoStream.Close();
                    }

                    decryptedBytes = memoryStream.ToArray();
                }
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        public static byte[] EncryptContent(string text)
        {
            byte[] decryptBytes = Encoding.UTF8.GetBytes(text);
            byte[] encryptedBytes = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(cryptKeyBytes, saltBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Mode = CipherMode.CBC;

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(decryptBytes, 0, decryptBytes.Length);
                        cryptoStream.Close();
                    }
                    encryptedBytes = memoryStream.ToArray();
                }
            }
            return encryptedBytes;
        }
    }
}