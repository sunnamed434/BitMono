Protections
===========

Maybe you want to run the same protection twice, or run protections in a different order. That's all
controlled by ``protections.json``.

Keep in mind:

- Execution order follows the position of each protection in ``protections.json``. ``AntiILdasm`` runs
  first here because it's listed first. The one exception is **Packers**, they always run last, after
  every other protection, no matter where you put them in the list.

By default everything is already configured the way it should be. If something doesn't behave the way you
want, you're free to enable, disable, reorder, or remove entries. Here's roughly how the default looks
(it can change between releases):


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