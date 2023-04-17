Configuration
=============

Injecting Configuration(s) in Protection constructor.
BitMono have such configuartions and all of them you can inject in your protection constructor:
- ProtectionSettings
- CriticalsSettings
- ObfuscationSettings


Here's example how to do that:


.. code-block:: csharp


	public class MagicProtection : Protection
	{
		private readonly ProtectionSettings _protectionSettings;
		private readonly CriticalsSettings _criticalsSettings;
		private readonly ObfuscationSettings _obfuscationSettings;

		public MagicProtection(
			IOptions<ProtectionSettings> protectionSettings,
			IOptions<CriticalsSettings> criticalsSettings,
			IOptions<ObfuscationSettings> obfuscationSettings,
			IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_protectionSettings = protectionSettings.Value;
			_criticalsSettings = criticalsSettings.Value;
			_obfuscationSettings = obfuscationSettings.Value;
		}	
	}