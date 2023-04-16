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
    	private readonly ObfuscationSettings _obfuscationSettings;

    	public MagicProtection(ObfuscationSettings obfuscationSettings, ProtectionContext context) : base(context)
    	{
        	_obfuscationSettings = obfuscationSettings;
    	}
    }
