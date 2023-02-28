How to disable path masking?
============================

Open-up ``logging.json`` in the root of the downloaded BitMono, edit this file, and remove this:
.. code-block:: json

	"Enrich": [
            {
                "Name": "WithSensitiveDataMasking",
                "Args": {
                    "options": {
                        "MaskValue": "***\\",
                        "MaskProperties": [ "path", "directory", "file" ],
                        "MaskingOperators": [ "BitMono.Host.Extensions.PathMaskingOperator, BitMono.Host" ]
                    }
                }
            },
        ],


So, after edit ``logging.json`` looks like this:
.. code-block:: json


	{
	    "Serilog": {
	        "Using": [
	            "Serilog",
	            "Serilog.Sinks.Console",
	            "Serilog.Sinks.File",
	            "Serilog.Sinks.Async",
	            "Serilog.Enrichers.Sensitive"
	        ],
	        "WriteTo": [
	            {
	                "Name": "Async",
	                "Args": {
	                    "configure": [
	                        {
	                            "Name": "File",
	                            "Args": {
	                                "path": "logs/bitmono-{{date}}.log",
	                                "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}"
	                            }
	                        }
	                    ]
	                }
	            }
	        ],
	        "Enrich": [
	            "FromLogContext"
	        ],
	        "MinimumLevel": "Debug"
	    }
	}