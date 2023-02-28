namespace BitMono.API.Contexts;

public class ProtectionParameters
{
    public ProtectionParameters(List<IMetadataMember> members)
    {
        Members = members;
    }

    public List<IMetadataMember> Members { get; }
}