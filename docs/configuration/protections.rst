Configure Protections
=====================

Let's say you want to execute the same protection twice or even execute protections in a different orders, etc.

Use ``protections.json`` - by default all protections are configured as they should, if something works not as intentionally you always may disable something, enable and even remove.

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
	      "Name": "BitTimeDateStamp",
	      "Enabled": false
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