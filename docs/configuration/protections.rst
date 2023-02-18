Configure Protections
=====================

Let's say you want to execute the same protection twice or even execute protections in a different orders, etc.

Use ``protections.json`` - by default all protections are configured as they should, if something works not as intentionally you always may disable something or enable or even remove it.

.. code-block:: json

	{
	  "Protections": [
        {
	      "Name": "UnknownProtection", // this will be ignored and shown as unknown protection, because doesn't exist by default.
	      "Enabled": true
	    },
	    {
	      "Name": "AntiILdasm",
	      "Enabled": true
	    },
	    {
	      "Name": "AntiDe4dot",
	      "Enabled": false
	    },
	    {
	      "Name": "BitMono",
	      "Enabled": true
	    },
	    {
	      "Name": "BitDotNet",
	      "Enabled": true,
	    }
	  ]
	}