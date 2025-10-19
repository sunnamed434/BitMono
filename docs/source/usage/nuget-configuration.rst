NuGet Configuration
==================

This guide explains how to configure NuGet when using BitMono as a NuGet package dependency.

When Configuration is Needed
----------------------------

You need to configure NuGet if you encounter dependency resolution errors when trying to use BitMono packages. This happens when BitMono may use nightly versions of AsmResolver (which we may use when needed for critical fixes) that are only available in a custom feed, not on the default nuget.org.

Configuration Steps
-------------------

1. **Create NuGet.config in your project root:**

.. code-block:: xml

   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageSources>
       <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
       <add key="asmresolver-nightly" value="https://nuget.washi.dev/v3/index.json" />
     </packageSources>
   </configuration>

2. **Restore packages:**

.. code-block:: bash

   dotnet restore

That's it! Your project should now be able to resolve AsmResolver dependencies.

Available BitMono Packages
--------------------------

**Core Packages:**

- `BitMono.API <https://www.nuget.org/packages/BitMono.API/>`_ - Core interfaces and abstractions
- `BitMono.Core <https://www.nuget.org/packages/BitMono.Core/>`_ - Main obfuscation engine
- `BitMono.Protections <https://www.nuget.org/packages/BitMono.Protections/>`_ - Protection implementations
- `BitMono.Shared <https://www.nuget.org/packages/BitMono.Shared/>`_ - Shared utilities and models

**Host & Utilities:**

- `BitMono.Host <https://www.nuget.org/packages/BitMono.Host/>`_ - Application host framework
- `BitMono.Utilities <https://www.nuget.org/packages/BitMono.Utilities/>`_ - Helper functions
- `BitMono.Obfuscation <https://www.nuget.org/packages/BitMono.Obfuscation/>`_ - High-level obfuscation orchestrator
- `BitMono.Runtime <https://www.nuget.org/packages/BitMono.Runtime/>`_ - Runtime components

**Tools:**

- `BitMono.GlobalTool <https://www.nuget.org/packages/BitMono.GlobalTool/>`_ - .NET Global Tool