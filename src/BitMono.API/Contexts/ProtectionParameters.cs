namespace BitMono.API.Contexts;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class ProtectionParameters
{
    public ProtectionParameters(List<IMetadataMember> members)
    {
        Members = members;
    }

    public List<IMetadataMember> Members { get; }
}