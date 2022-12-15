namespace BitMono.API.Protecting.Contexts
{
    public class ProtectionParameters
    {
        public ProtectionParameters(List<IDnlibDef> targets)
        {
            Targets = targets;
        }

        public List<IDnlibDef> Targets { get; private set; }
    }
}