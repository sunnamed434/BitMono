Building BitMono
===============

Getting Binaries
---------------

If you just need the compiled binaries, grab them `from releases <https://github.com/sunnamed434/BitMono/releases>`_. Open the dropdown button `Assets` and pick the archive you want. These binaries are built automatically via CI/CD pipeline.

Building from Source
-------------------

Prerequisites
~~~~~~~~~~~~

Recommended to install tools via Visual Studio installer, otherwise you can grab those tools directly via the links below:

- `Visual Studio 2022 <https://visualstudio.microsoft.com/downloads>`_ or `JetBrains Rider <https://www.jetbrains.com/rider/download>`_ or newer
- `.NET Framework 4.6.2 <https://dotnet.microsoft.com/en-us/download/dotnet-framework/net462>`_
- `.NET 10.0 <https://dotnet.microsoft.com/en-us/download/dotnet/10.0>`_
- `.NET 9.0 <https://dotnet.microsoft.com/en-us/download/dotnet/9.0>`_
- `.NET 8.0 <https://dotnet.microsoft.com/en-us/download/dotnet/8.0>`_
- `.NET 7.0 <https://dotnet.microsoft.com/en-us/download/dotnet/7.0>`_
- `.NET 6.0 <https://dotnet.microsoft.com/en-us/download/dotnet/6.0>`_

Building
~~~~~~~~

To build the solution from command line:

.. code-block:: console

   dotnet build

Or just use the IDE `Build` button if you have one.

Testing
~~~~~~~

To run tests:

.. code-block:: console

   dotnet test

Release Archives
---------------

Archive examples (versions and naming might be different):

- .NET 10.0: ``BitMono-v0.24.2+7aaeceac-CLI-net10.0-linux-x64.zip``
- .NET 9.0: ``BitMono-v0.24.2+7aaeceac-CLI-net9.0-linux-x64.zip``
- .NET 8.0: ``BitMono-v0.24.2+7aaeceac-CLI-net8.0-linux-x64.zip``
- .NET 7.0: ``BitMono-v0.24.2+7aaeceac-CLI-net7.0-win-x64.zip``
- .NET 6.0: ``BitMono-v0.24.2+7aaeceac-CLI-net6.0-linux-x64.zip``
- .NET 462: ``BitMono-v0.24.2+7aaeceac-CLI-net462-win-x64.zip``
- netstandard 2.1: ``BitMono-v0.24.2+7aaeceac-CLI-netstandard2.1-linux-x64.zip``
- netstandard 2.0: ``BitMono-v0.24.2+7aaeceac-CLI-netstandard2.0-win-x64.zip``

Archive naming explained:

- ``v0.24.2`` is the version
- ``+7aaeceac`` after the version is the commit hash
- ``CLI`` means command line interface (currently BitMono only has CLI)
- ``net10.0``, ``net9.0``, ``net8.0``, etc. is the target framework BitMono was built on

Getting Help
-----------

If you run into issues or have questions:

- Ask them `here <https://github.com/sunnamed434/BitMono/issues>`_
- Email: sunnamed434 (at) proton.me 