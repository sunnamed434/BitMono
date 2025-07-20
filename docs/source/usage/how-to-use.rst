How To Use
==========

This guide shows you how to install and use BitMono to obfuscate your .NET applications.

Command Line Usage
-----------------

Basic Usage
~~~~~~~~~~

Just run BitMono without any arguments and it'll guide you through the process:

.. code-block:: console

   BitMono.CLI

It'll ask you for your file, what protections to use, and where your dependencies are.

Example interactive session:

.. code-block:: console

   Please, specify file or drag-and-drop in BitMono CLI
   C:\MyApp\MyApplication.exe
   File successfully specified: C:\MyApp\MyApplication.exe
   
   No protection is enabled in protections.json file, please either enable any protection first or input the preferred with ',' delimiter, example: StringsEncryption,AntiDe4dot
   FullRenamer,StringEncryption,BitDotNet
   Protections successfully specified: FullRenamer, StringEncryption, BitDotNet
   
   Dependencies (libs) directory was automatically found in: C:\MyApp\libs

Command Line Mode
~~~~~~~~~~~~~~~~

For automation or scripting, you can also use command line arguments:

Here are the available options:

.. code-block:: console

   BitMono.CLI [options]

Options:

- ``-f, --file``: **Required**. Path to the file you want to obfuscate
- ``-l, --libraries``: Path to the libraries directory (defaults to ``libs``)
- ``-o, --output``: Output directory path (defaults to ``output``)
- ``-p, --protections``: Specify protections to enable (can also be configured in ``protections.json``)
- ``--help``: Display help information
- ``--version``: Display version information

Examples
~~~~~~~~

Basic obfuscation with default settings:

.. code-block:: console

   BitMono.CLI -f C:\MyApp\MyApplication.exe

Specify custom libraries directory:

.. code-block:: console

   BitMono.CLI -f C:\MyApp\MyApplication.exe -l C:\MyApp\Dependencies

Specify output directory:

.. code-block:: console

   BitMono.CLI -f C:\MyApp\MyApplication.exe -o C:\MyApp\Obfuscated

Complete example with all options:

.. code-block:: console

   BitMono.CLI -f C:\MyApp\MyApplication.exe -l C:\MyApp\Dependencies -o C:\MyApp\Obfuscated

Installation
-----------

You can get BitMono in a few different ways:

GitHub Releases
~~~~~~~~~~~~~~~

Download the latest release from `GitHub <https://github.com/sunnamed434/BitMono/releases/latest>`_:

1. Go to the latest BitMono release page
2. Select the appropriate archive for your target framework:

   - **For .NET 8 applications**: ``BitMono-v0.25.3+e64e54d3-CLI-net8.0-win-x64.zip``
   - **For .NET Framework applications**: ``BitMono-v0.25.3+e64e54d3-CLI-net462-win-x64.zip``
   - **For .NET Standard applications**: Use either .NET 8 or .NET Framework version
   - **For Mono or Unity Engine Runtime**: Use the .NET Framework version (``BitMono-v0.25.3+e64e54d3-CLI-net462-win-x64.zip``)

3. Extract the archive to your desired location
4. The extracted folder contains the BitMono CLI executable

.. note::
   Choose the version that matches your target framework to avoid compatibility issues.

.. note::
   By default, all protections are disabled in ``protections.json``. You can either enable them in the configuration file or specify them interactively when prompted.

.NET Global Tool
~~~~~~~~~~~~~~~

Install BitMono as a .NET global tool:

.. code-block:: console

   dotnet tool install --global BitMono.GlobalTool

After installation, use the tool with:

.. code-block:: console

   bitmono.console [options]

NuGet Packages
~~~~~~~~~~~~~~

BitMono is also available as NuGet packages for integration into your projects:

- **BitMono.Obfuscation**: `Core obfuscation engine <https://www.nuget.org/packages/BitMono.Obfuscation>`_ - Main obfuscation engine with pipeline and protection execution
- **BitMono.Core**: `Core framework and base classes <https://www.nuget.org/packages/BitMono.Core>`_ - Core framework with AsmResolver integration and base protection classes
- **BitMono.API**: `API for custom protections <https://www.nuget.org/packages/BitMono.API>`_ - Interfaces and base classes for building custom protections
- **BitMono.Protections**: `Built-in protection implementations <https://www.nuget.org/packages/BitMono.Protections>`_ - Collection of ready-to-use obfuscation protections
- **BitMono.Shared**: `Shared models and configuration <https://www.nuget.org/packages/BitMono.Shared>`_ - Common models, settings, and configuration classes
- **BitMono.Utilities**: `Utility classes and extensions <https://www.nuget.org/packages/BitMono.Utilities>`_ - Helper classes for assembly manipulation and common utilities
- **BitMono.Runtime**: `Runtime components <https://www.nuget.org/packages/BitMono.Runtime>`_ - Runtime-specific functionality and unsafe code operations

Directory Structure
------------------

BitMono needs your files organized like this:

.. code-block:: text

   your_obfuscation_folder/
   ├── your_app.exe
   └── libs/
       ├── ImportantLibrary.dll
       ├── SuperImportantLibrary.dll
       └── ...

The ``libs`` directory contains the dependencies (.dll files) that your application uses. BitMono needs these to understand what your code calls - like `Console.WriteLine()` or methods from other libraries. Without them, BitMono might not understand your code's dependencies and could break something.

.. note::
   The ``libs`` directory is optional. If your app doesn't use external dependencies or if BitMono can find them automatically, you can skip this step.

Setup Steps
~~~~~~~~~~

1. Create a folder for your obfuscation project
2. Copy your main executable to this folder
3. (Optional) Create a ``libs`` subfolder and copy your dependencies (.dll files) there
4. Run BitMono on your executable

.. note::
   If you use a custom libraries directory name, specify it with the ``-l`` option.
   BitMono looks for a ``libs`` directory by default.

Configuration
-------------

BitMono uses JSON configuration files to control its behavior. These files are in the same directory as the BitMono executable.

Obfuscation Configuration
~~~~~~~~~~~~~~~~~~~~~~~~~

The ``obfuscation.json`` file controls general obfuscation settings:

.. code-block:: json

   {
     "Watermark": true,
     "ClearCLI": false,
     "ForceObfuscation": false,
     "ReferencesDirectoryName": "libs",
     "OutputDirectoryName": "output",
     "NotifyProtections": true,
     "NoInliningMethodObfuscationExclude": true,
     "ObfuscationAttributeObfuscationExclude": true,
     "ObfuscateAssemblyAttributeObfuscationExclude": true,
     "ReflectionMembersObfuscationExclude": true,
     "StripObfuscationAttributes": true,
     "OutputPEImageBuildErrors": true,
     "FailOnNoRequiredDependency": false,
     "OutputRuntimeMonikerWarnings": true,
     "OutputConfigureForNativeCodeWarnings": true,
     "OpenFileDestinationInFileExplorer": false
   }

Key Settings:

- **Watermark**: Adds visible indicators that BitMono was used
- **ClearCLI**: Clears the console when obfuscation starts
- **ReferencesDirectoryName**: Name of the libraries directory (default: ``libs``)
- **OutputDirectoryName**: Name of the output directory (default: ``output``)
- **OpenFileDestinationInFileExplorer**: Opens output directory after completion

Protections Configuration
~~~~~~~~~~~~~~~~~~~~~~~~

The ``protections.json`` file controls which protections are enabled:

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

Enabling Protections
~~~~~~~~~~~~~~~~~~~

To enable a protection, set its ``Enabled`` property to ``true`` in the ``protections.json`` file. You need at least one protection enabled for obfuscation to work.

You can also enable protections via command line:

.. code-block:: console

   BitMono.CLI -f MyApp.exe -p FullRenamer,StringEncryption

Criticals Configuration
~~~~~~~~~~~~~~~~~~~~~~

The ``criticals.json`` file defines types, methods, and interfaces that shouldn't be obfuscated:

.. code-block:: json

   {
     "Criticals": {
       "Types": [
         "System.String",
         "System.Int32"
       ],
       "Methods": [
         "System.Console.WriteLine",
         "System.Console.ReadLine"
       ],
       "Interfaces": [
         "System.IDisposable"
       ]
     }
   }

.. note::
   Use criticals when you don't want BitMono to touch certain parts of your code. For example, if your app uses reflection to call methods by name, you'd add those method names to criticals so they don't get renamed.

Basic Workflow
-------------

Here's the typical process:

1. **Prepare Your Application**: Build your .NET application and note the target framework

2. **Download BitMono**: Choose the right version for your framework and extract it

3. **Set Up Project Structure**: Create a folder, copy your executable there, and optionally add a ``libs`` folder with dependencies

4. **Configure Protections**: Edit ``protections.json`` to enable the protections you want

5. **Run Obfuscation**: Use interactive mode (just run ``BitMono.CLI``), command line, or drag and drop

6. **Test the Result**: Check the ``output`` folder and test your obfuscated app

Troubleshooting
--------------

Common Issues
~~~~~~~~~~~~

**Missing Dependencies**: Make sure all required .dll files are in the ``libs`` directory.

**Framework Mismatch**: Use the BitMono version that matches your target framework.

**No Protections Enabled**: You need at least one protection enabled in ``protections.json``.

**Permission Errors**: Make sure BitMono has write permissions to the output directory.

For more detailed troubleshooting information, see the `troubleshooting guide <troubleshooting.html>`_.

Next Steps
----------

- Read about `available protections <../protection-list/overview.html>`_
- Learn about `configuration options <../configuration/protections.html>`_
- Check `best practices <../bestpractices/zero-risk-obfuscation.html>`_
- Explore `developer documentation <../developers/first-protection.html>`_ for custom protections
- See the `troubleshooting guide <troubleshooting.html>`_ for common issues and solutions
