using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BitMono.Encryption
{
    public static class Encryptor
    {
        private static byte[] cryptKeyBytes = new byte[8];
        private static byte[] saltBytes = new byte[8];

        internal static string Decrypt(byte[] bytes)
        {
            byte[] decryptedBytes = null;
            using (var memoryStream = new MemoryStream())
            {
                using (var aes = new RijndaelManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    var key = new Rfc2898DeriveBytes(cryptKeyBytes, saltBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Mode = CipherMode.CBC;

                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(bytes, 0, bytes.Length);
                        cryptoStream.Close();
                    }
                    decryptedBytes = memoryStream.ToArray();
                    key.Dispose();
                }
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        public static byte[] EncryptContent(string text, byte[] saltBytes, byte[] cryptKeyBytes)
        {
            var decryptBytes = Encoding.UTF8.GetBytes(text);
            byte[] encryptedBytes = null;
            using (var memoryStream = new MemoryStream())
            {
                using (var aes = new RijndaelManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    var key = new Rfc2898DeriveBytes(saltBytes, cryptKeyBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Mode = CipherMode.CBC;

                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(decryptBytes, 0, decryptBytes.Length);
                        cryptoStream.Close();
                    }
                    encryptedBytes = memoryStream.ToArray();
                    key.Dispose();
                }
            }
            return encryptedBytes;
        }
    }
}