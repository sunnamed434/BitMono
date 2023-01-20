namespace BitMono.Runtime;

public struct Data
{
    internal static readonly byte[] CryptKeyBytes = new byte[8];
    internal static readonly byte[] SaltBytes = new byte[8];
    
    static Data()
    {
        CryptKeyBytes = new byte[8];
        SaltBytes = new byte[8];
    }
}