Assembly Signing
================

BitMono supports strong name signing of assemblies. You can configure signing using either the CLI option or the configuration file.

CLI Option
----------

Use the ``--strong-name-key`` option:

.. code-block:: console

   BitMono.CLI -f MyApp.exe --strong-name-key MyKey.snk

Configuration File
------------------

Add the key file path to your ``obfuscation.json``:

.. code-block:: json

   {
     "StrongNameKeyFile": "MyKey.snk"
   }
