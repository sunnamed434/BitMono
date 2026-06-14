Configuration
=============

Need BitMono's settings inside your protection? Inject any of these through your constructor and the DI
container hands them to you:

- ``ProtectionSettings``
- ``CriticalsSettings``
- ``ObfuscationSettings``

Here's how:


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