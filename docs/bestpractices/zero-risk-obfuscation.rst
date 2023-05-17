Zero Risk of Obfuscation
========================

You may have a project which is really heavy and you don't want to loose any perfomance of it, so, well, you can't really have zero losses of perfomance but you can minimize it, due to BitMono is not really stable with all runtimes and this is very complex obfuscation, would really recommend to use this, as we call it `BitMono Starter Pack for Mono` - like in Fortnite :D
 
1. AntiDecompiler
2. BitMethodDotnet
3. BitTimeDateStamp
4. BitDotNet
5. BitMono
6. (Not optional) StringsEncryption, if you really-really need to encrypt strings, this may very-very-very harmful to the application's performance


`protections.json` will look like this:

.. code-block:: json

	{
	  "Protections": [
	    {
	      "Name": "AntiILdasm",
	      "Enabled": false
	    },
	    {
	      "Name": "AntiDe4dot",
	      "Enabled": false
	    },
	    {
	      "Name": "ObjectReturnType",
	      "Enabled": false
	    },
	    {
	      "Name": "NoNamespaces",
	      "Enabled": false
	    },
	    {
	      "Name": "FullRenamer",
	      "Enabled": false
	    },
	    {
	      "Name": "AntiDebugBreakpoints",
	      "Enabled": false
	    },
	    {
	      "Name": "StringsEncryption",
	      "Enabled": false
	    },
	    {
	      "Name": "UnmanagedString",
	      "Enabled": false
	    },
	    {
	      "Name": "DotNetHook",
	      "Enabled": false
	    },
	    {
	      "Name": "CallToCalli",
	      "Enabled": false
	    },
	    {
	      "Name": "AntiDecompiler",
	      "Enabled": true
	    },
	    {
	      "Name": "BitMethodDotnet",
	      "Enabled": true
	    },
	    {
	      "Name": "BitTimeDateStamp",
	      "Enabled": true
	    },
	    {
	      "Name": "BitDotNet",
	      "Enabled": true
	    },
	    {
	      "Name": "BitMono",
	      "Enabled": true
	    }
	  ]
	}