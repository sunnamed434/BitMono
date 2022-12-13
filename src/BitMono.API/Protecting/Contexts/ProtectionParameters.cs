using dnlib.DotNet;
using System.Collections.Generic;

namespace BitMono.Core.Protecting
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