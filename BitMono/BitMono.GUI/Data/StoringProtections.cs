using BitMono.API.Configuration;
using BitMono.Core.Configuration.Extensions;
using BitMono.Core.Models;
using BitMono.GUI.API;
using Microsoft.Extensions.Configuration;

namespace BitMono.GUI.Data
{
	internal class StoringProtections : IStoringProtections
	{
		private readonly IConfiguration m_Configuration;

		public StoringProtections(IBitMonoProtectionsConfiguration configuration)
		{
			m_Configuration = configuration.Configuration;
			Protections = m_Configuration.GetProtectionSettings().ToList();
		}

		public IList<ProtectionSettings> Protections { get; }
	}
}