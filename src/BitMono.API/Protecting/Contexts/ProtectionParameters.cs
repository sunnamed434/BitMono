namespace BitMono.API.Protecting.Contexts
{
    public class ProtectionParameters
    {
        public ProtectionParameters(List<IMetadataMember> targets)
        {
            Targets = targets;
        }

        public List<IMetadataMember> Targets { get; private set; }
    }
}