NuGet Configuration
===================

You only need this if you hit dependency errors while using BitMono as a NuGet package. BitMono
sometimes pulls a nightly build of AsmResolver (when we need a critical fix that isn't on nuget.org
yet), and those live on Washi's feed, not the default one. If NuGet can't resolve AsmResolver, point it
at that feed too.

Add a ``NuGet.config`` to your project root:

.. code-block:: xml

   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageSources>
       <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
       <add key="asmresolver-nightly" value="https://nuget.washi.dev/v3/index.json" />
     </packageSources>
   </configuration>

Then restore:

.. code-block:: bash

   dotnet restore

That's it, AsmResolver should resolve now.

The packages
------------

Most people only ever need one of these:

- `BitMono.Integration <https://www.nuget.org/packages/BitMono.Integration/>`_ - obfuscate your project
  on every build by adding one ``<PackageReference>``. See :doc:`msbuild-integration`.
- `BitMono.GlobalTool <https://www.nuget.org/packages/BitMono.GlobalTool/>`_ - the BitMono CLI as a .NET
  global tool (``dotnet tool install --global BitMono.GlobalTool``). See :doc:`how-to-use`.

The rest are the engine packages, for building your own tools on top of BitMono (see
:doc:`../developers/configuration`):

- `BitMono.API <https://www.nuget.org/packages/BitMono.API/>`_ - core interfaces and abstractions
- `BitMono.Core <https://www.nuget.org/packages/BitMono.Core/>`_ - the obfuscation engine
- `BitMono.Protections <https://www.nuget.org/packages/BitMono.Protections/>`_ - the protection implementations
- `BitMono.Shared <https://www.nuget.org/packages/BitMono.Shared/>`_ - shared utilities and models
- `BitMono.Host <https://www.nuget.org/packages/BitMono.Host/>`_ - application host framework
- `BitMono.Utilities <https://www.nuget.org/packages/BitMono.Utilities/>`_ - helper functions
- `BitMono.Obfuscation <https://www.nuget.org/packages/BitMono.Obfuscation/>`_ - high-level obfuscation orchestrator
- `BitMono.Runtime <https://www.nuget.org/packages/BitMono.Runtime/>`_ - runtime components for obfuscated assemblies
