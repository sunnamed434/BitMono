namespace BitMono.Runtime;

public struct Decryptor
{
    internal static string Decrypt(byte[] bytes, byte[] saltBytes, byte[] cryptKeyBytes)
    {
        byte[]? decryptedBytes = null;
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
}