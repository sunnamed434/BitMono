Protections
===========

Let's say you want to execute the same protection twice or even execute protections in a different orders, etc.

Keep in mind:

- The order of execution is determined by the position of each protection in the ``protections.json`` file within the configuration file. For example, AntiILdasm is executed first (because this protection is first in configuration) and Packers always run last after all protections, even if you set this ``Packer`` protection as a first one in configuration it will anyway gonna be called last.

Use ``protections.json`` - by default all protections are configured as they should, if something works not as intentionally you always may disable something, enable and even remove.
Default config (it can be different with new updates, but just let's see how it looks like):


.. code-block:: json

	{
	  "Protections": [
	    {
	      "Name": "AntiILdasm", // This is going to be executed first
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
	      "Enabled": true
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
	      "Name": "DotNetHook",
	      "Enabled": false
	    },
	    {
	      "Name": "CallToCalli",
	      "Enabled": false
	    },
	    {
	      "Name": "AntiDecompiler",
	      "Enabled": false
	    },
	    {
	      "Name": "BitMethodDotnet",
	      "Enabled": false
	    },
	    {
	      "Name": "BitTimeDateStamp", // Packer
	      "Enabled": false
	    },
	    {
	      "Name": "BitDotNet", // Packer
	      "Enabled": true
	    },
	    {
	      "Name": "BitMono", // This is going to be executed last because its a Packer, even if you put this before AntiILdasm it going to be called last anyway.
	      "Enabled": true
	    }
	  ]
	}