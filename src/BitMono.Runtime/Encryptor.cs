namespace BitMono.Runtime;

public struct Encryptor
{
    internal static byte[] EncryptContent(string text, byte[] saltBytes, byte[] cryptKeyBytes)
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