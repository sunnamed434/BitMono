Building BitMono
================

BitMono's binaries are built only by our GitHub CI/CD pipeline and published to
`GitHub Releases <https://github.com/sunnamed434/BitMono/releases>`_. We don't build or distribute them
anywhere else, so the releases page is always the source of truth for an official build.

Just want to run BitMono?
-------------------------

Grab a prebuilt binary from `GitHub Releases <https://github.com/sunnamed434/BitMono/releases>`_. Open the
**Assets** dropdown on the release you want and pick the archive that matches your target (see
:doc:`../usage/how-to-use` if you're not sure which one). No need to build anything yourself.

About the release archives
--------------------------

Every release on GitHub ships a set of ``.zip`` archives, one per target framework. The names look like
this (versions and platforms vary):

.. code-block:: text

   BitMono-v0.41.1+7aaeceac-CLI-net8.0-win-x64.zip

Reading it left to right:

- ``v0.41.1`` — the version
- ``+7aaeceac`` — the commit it was built from
- ``CLI`` — the command-line interface (BitMono is CLI-only for now)
- ``net8.0`` — the target framework the build runs on (you'll also see ``net462``, ``net9.0``, ``net10.0``,
  ``netstandard2.1``, and so on)

Pick the framework that matches what you're obfuscating. Targeting .NET Framework or Unity/Mono? Use the
``net462`` archive.

Building from source
--------------------

You only need this if you want to hack on BitMono itself.

Prerequisites
~~~~~~~~~~~~~

Easiest to install through the Visual Studio installer, or grab them directly from the links below:

- `Visual Studio 2022 <https://visualstudio.microsoft.com/downloads>`_ or
  `JetBrains Rider <https://www.jetbrains.com/rider/download>`_ (or newer)
- `.NET Framework 4.6.2 <https://dotnet.microsoft.com/en-us/download/dotnet-framework/net462>`_
- .NET `6.0 <https://dotnet.microsoft.com/en-us/download/dotnet/6.0>`_,
  `7.0 <https://dotnet.microsoft.com/en-us/download/dotnet/7.0>`_,
  `8.0 <https://dotnet.microsoft.com/en-us/download/dotnet/8.0>`_,
  `9.0 <https://dotnet.microsoft.com/en-us/download/dotnet/9.0>`_, and
  `10.0 <https://dotnet.microsoft.com/en-us/download/dotnet/10.0>`_

Build and test
~~~~~~~~~~~~~~

.. code-block:: console

   dotnet build
   dotnet test

Or just hit the **Build** button in your IDE.

Getting help
------------

Run into something or have a question?

- Open an issue `here <https://github.com/sunnamed434/BitMono/issues>`_
- Email: sunnamed434 (at) proton.me
