using BitMono.Core.Models;
using System.Collections.Generic;

namespace BitMono.GUI.API
{
    public interface IStoringProtections
    {
        IList<ProtectionSettings> Protections { get; }
    }
}