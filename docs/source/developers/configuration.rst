Configuration
=============

Injecting Configuration(s) in Protection constructor.
BitMono have such configurations and all of them you can inject in your protection constructor:

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
			ProtectionSettings protectionSettings,
			CriticalsSettings criticalsSettings,
			ObfuscationSettings obfuscationSettings,
			IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
		{
			_protectionSettings = protectionSettings;
			_criticalsSettings = criticalsSettings;
			_obfuscationSettings = obfuscationSettings;
		}
	}