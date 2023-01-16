using BitMono.Core.Extensions;

namespace BitMono.GUI.Data;

internal class StoringProtections : IStoringProtections
{
	public StoringProtections(IBitMonoProtectionsConfiguration configuration)
	{
		Protections = configuration.GetProtectionSettings();
	}

	public List<ProtectionSetting> Protections { get; }
}