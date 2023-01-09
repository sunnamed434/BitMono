namespace BitMono.Runtime;

public struct Data
{
    internal readonly static byte[] CryptKeyBytes = new byte[8];
    internal readonly static byte[] SaltBytes = new byte[8];
    
    static Data()
    {
        CryptKeyBytes = new byte[8];
        SaltBytes = new byte[8];
    }
}