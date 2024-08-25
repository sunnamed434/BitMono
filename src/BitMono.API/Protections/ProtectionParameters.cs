namespace BitMono.API.Protections;

public class ProtectionParameters
{
    public ProtectionParameters(List<IMetadataMember> members)
    {
        Members = members;
    }

    public List<IMetadataMember> Members { get; }
}