using BitMono.API.Configuration;
using BitMono.Core.Extensions.Configuration;
using BitMono.Core.Models;
using BitMono.GUI.API;
using Microsoft.Extensions.Configuration;

namespace BitMono.GUI.Data
{
    internal class StoringProtections : IStoringProtections
	{
		public StoringProtections(IBitMonoProtectionsConfiguration configuration)
		{
			Protections = configuration.GetProtectionSettings();
		}

		public List<ProtectionSettings> Protections { get; }
	}
}