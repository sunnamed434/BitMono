namespace BitMono.GUI.Data;

internal class StoringProtections : IStoringProtections
{
	public StoringProtections(IOptions<ProtectionSettings> settings)
	{
		Protections = settings.Value.Protections;
	}

	public List<ProtectionSetting> Protections { get; }
}