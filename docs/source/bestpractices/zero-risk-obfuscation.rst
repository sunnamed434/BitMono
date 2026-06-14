Zero Risk of Obfuscation
========================

Got a heavy project and don't want to lose performance? You can't get *zero* losses, but you can keep
them tiny. BitMono isn't equally stable on every runtime and some of its protections are heavy, so here's
a safe, low-risk set, we like to call it the `BitMono Starter Pack for Mono` - like in Fortnite :D

1. AntiDecompiler
2. BitMethodDotnet
3. BitTimeDateStamp
4. BitDotNet
5. BitMono
6. StringsEncryption — *optional*. Only add it if you really need encrypted strings, it can hurt
   performance a lot.

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