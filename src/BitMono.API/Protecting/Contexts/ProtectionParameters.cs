namespace BitMono.API.Protecting.Contexts
{
    public class ProtectionParameters
    {
        public ProtectionParameters(List<IMemberDefinition> targets)
        {
            Targets = targets;
        }

        public List<IMemberDefinition> Targets { get; private set; }
    }
}