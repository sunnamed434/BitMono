How To Use
==========

Choose your integration:

CLI Tool
--------

Download and run BitMono from the command line.

Download
~~~~~~~~

Get BitMono from `GitHub releases <https://github.com/sunnamed434/BitMono/releases/latest>`_:

- .NET 8 apps: ``BitMono-v0.25.3-CLI-net8.0-win-x64.zip``
- .NET Framework apps: ``BitMono-v0.25.3-CLI-net462-win-x64.zip``
- Unity/Mono: Use the .NET Framework version

Usage
~~~~~

.. code-block:: console

   BitMono.CLI -f MyApp.exe

More options:

.. code-block:: console

   BitMono.CLI -f MyApp.exe -l Dependencies -o MyObfuscatedApp -p FullRenamer,StringEncryption

Available options:

- ``-f, --file``: File to obfuscate (required)
- ``-l, --libraries``: Dependencies folder (default: ``libs``)
- ``-o, --output``: Output folder (default: ``output``)
- ``-p, --protections``: Which protections to use
- ``--protections-file``: Custom protections config file
- ``--criticals-file``: Custom criticals config file
- ``--logging-file``: Custom logging config file
- ``--obfuscation-file``: Custom obfuscation config file
- ``--no-watermark``: Turn off watermarking

Setup
~~~~~

Put your files like this:

.. code-block:: text

   my_project/
   ├── MyApp.exe
   └── libs/
       ├── SomeLibrary.dll
       └── AnotherLibrary.dll

The ``libs`` folder has your app's dependencies. BitMono needs these to understand your code.

.NET Global Tool
----------------

Install and use BitMono as a global .NET tool.

Installation
~~~~~~~~~~~~

.. code-block:: console

   dotnet tool install --global BitMono.GlobalTool

Usage
~~~~~

Same as CLI tool, just run ``BitMono`` instead of ``BitMono.CLI``.

Configuration
-------------

BitMono uses these config files:

``protections.json`` - Which protections to use:

.. code-block:: text

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
         "Name": "FullRenamer",
         "Enabled": true
       },
       {
         "Name": "StringsEncryption",
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

.. note::

   The order of protections in the configuration determines their execution order. 
   Packers (like BitDotNet and BitMono) always run last, regardless of their position in the configuration.

``criticals.json`` - What NOT to obfuscate:

.. code-block:: text

   {
     "UseCriticalAttributes": true,
     "CriticalAttributes": [
       {
         "Namespace": "UnityEngine",
         "Name": "SerializeField"
       }
     ],
     "UseCriticalModelAttributes": true,
     "CriticalModelAttributes": [
       {
         "Namespace": "System",
         "Name": "SerializableAttribute"
       }
     ],
     "UseCriticalInterfaces": true,
     "CriticalInterfaces": [
       "IRocketPlugin",
       "IOpenModPlugin"
     ],
     "UseCriticalBaseTypes": true,
     "CriticalBaseTypes": [
       "RocketPlugin*",
       "OpenModUnturnedPlugin*"
     ],
     "UseCriticalMethods": true,
     "CriticalMethods": [
       "Awake",
       "Start",
       "Update",
       "OnDestroy"
     ],
     "UseCriticalMethodsStartsWith": true,
     "CriticalMethodsStartsWith": [
       "OV_"
     ]
   }

This file controls what gets excluded from obfuscation:

- **CriticalAttributes** - Exclude members with specific attributes
- **CriticalModelAttributes** - Exclude types with serialization attributes  
- **CriticalInterfaces** - Exclude types that inherit specific interfaces
- **CriticalBaseTypes** - Exclude types that inherit specific base types (supports glob patterns)
- **CriticalMethods** - Exclude methods by exact name
- **CriticalMethodsStartsWith** - Exclude methods that start with specific strings

You can use glob patterns (``*``) in base types and method patterns.

``obfuscation.json`` - General settings:

.. code-block:: text

   {
     "Watermark": true,
     "OutputDirectoryName": "output"
   }

Most settings have sensible defaults. You only need to change them if you want something different.

Unity Integration
----------------

BitMono includes Unity integration that automatically obfuscates your assemblies during the Unity build process. 
The integration hooks into Unity's build pipeline and runs BitMono CLI to protect your game code.

.. note::

   IL2CPP is not supported yet, however is planned to be supported in the future.

Installation
~~~~~~~~~~~

Download the Unity Package
~~~~~~~~~~~~~~~~~~~~~~~~~~

1. Go to the latest BitMono release on `GitHub <https://github.com/sunnamed434/BitMono/releases/latest>`_
2. Download the Unity package for your Unity version:

   - **Unity 2019.4**: ``BitMono-Unity-v0.25.3-Unity2019.4.40f1.unitypackage``
   - **Unity 2020.3**: ``BitMono-Unity-v0.25.3-Unity2020.3.48f1.unitypackage``
   - **Unity 2021.3**: ``BitMono-Unity-v0.25.3-Unity2021.3.45f1.unitypackage``
   - **Unity 2022.3**: ``BitMono-Unity-v0.25.3-Unity2022.3.50f1.unitypackage``
   - **Unity 6.0+**: ``BitMono-Unity-v0.25.3-Unity6.0.0f1.unitypackage``

3. In Unity, go to **Assets → Import Package → Custom Package**
4. Select the downloaded ``.unitypackage`` file
5. Click **Import** to add BitMono to your project

Project Structure
~~~~~~~~~~~~~~~~~

After importing, your project will contain:

.. code-block:: text

   Assets/
   ├── BitMono.Unity/
   │   ├── Editor/
   │   │   ├── BitMonoBuildProcessor.cs    # Build hook implementation
   │   │   ├── BitMonoConfig.cs            # Configuration ScriptableObject
   │   │   ├── BitMonoConfigInspector.cs   # Unity Inspector UI
   │   │   └── BitMono.Unity.Editor.asmdef # Assembly definition
   │   ├── BitMonoConfig.asset             # Your configuration file
   │   └── package.json                    # Unity Package Manager metadata
   └── BitMono.CLI/
       ├── BitMono.CLI.exe                 # The actual obfuscation tool
       ├── protections.json                # Protection settings
       ├── obfuscation.json                # Obfuscation settings
       ├── criticals.json                  # What not to obfuscate
       └── logging.json                    # Logging configuration

Configuration
~~~~~~~~~~~~~

1. In Unity, go to **Window → BitMono → Configuration**
2. Check **Enable Obfuscation** to turn on BitMono
3. That's it! BitMono will automatically protect your code during builds

The integration comes with sensible defaults. You only need to change settings if you want something different.

Usage
~~~~~

Just build your project normally:

1. Go to **File → Build Settings → Build**
2. BitMono automatically obfuscates your code during the build
3. Your final build contains protected code

That's it! No extra steps needed.

Troubleshooting
--------------

For detailed troubleshooting information, see the `troubleshooting guide <troubleshooting.html>`_.

Next Steps
----------

- Read about `available protections <../protection-list/overview.html>`_
- Learn about `configuration options <../configuration/protections.html>`_
- Check `best practices <../bestpractices/zero-risk-obfuscation.html>`_
- Explore `developer documentation <../developers/first-protection.html>`_ for custom protections
