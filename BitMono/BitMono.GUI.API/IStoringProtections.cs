using BitMono.Core.Models;
using System.Collections.Generic;

namespace BitMono.GUI.API
{
    public interface IStoringProtections
    {
        List<ProtectionSettings> Protections { get; }
    }
}