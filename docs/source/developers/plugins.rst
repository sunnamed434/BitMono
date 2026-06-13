Plugins
=======

BitMono can load **plugins** - external assemblies that contribute your own protections - without
rebuilding BitMono. Drop a plugin DLL into the plugins directory and BitMono discovers and runs the
protections inside it, exactly like the built-in ones. (`#227 <https://github.com/sunnamed434/BitMono/issues/227>`_)

.. warning::

    Plugins are loaded into the BitMono process with full trust - .NET cannot sandbox them. Only drop
    in plugins from sources you trust, just as you would with any other executable.


Where plugins live
------------------

By default BitMono scans a ``plugins`` directory next to the BitMono executable. You can change the
directory name (or point it at an absolute path) with ``PluginsDirectoryName`` in ``obfuscation.json``:

.. code-block:: json

    {
      "PluginsDirectoryName": "plugins"
    }

Two layouts are supported:

- **Flat** - drop the DLL straight into the directory: ``plugins/MyProtections.dll``.
- **Per-plugin folder** - give each plugin its own folder: ``plugins/MyProtections/MyProtections.dll``.
  Put any extra dependencies in a nested folder (for example ``plugins/MyProtections/libs/``); they are
  resolved on demand and are not themselves treated as plugins.

If the directory does not exist, BitMono simply skips plugin loading.


Writing a plugin
----------------

A plugin is an ordinary class library that references ``BitMono.API`` (and usually ``BitMono.Core`` for
the ``Protection`` base class) and exposes one or more public protections. Writing a protection is the
same as :doc:`creating a built-in one <first-protection>`:

.. code-block:: csharp

    using BitMono.Core;
    using BitMono.Core.Attributes;
    using BitMono.Shared.DependencyInjection;
    using BitMono.Shared.Logging;

    [ProtectionName("HelloPlugin")]
    public class HelloPlugin : Protection
    {
        private readonly ILogger _logger;

        public HelloPlugin(IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger>().ForContext<HelloPlugin>();
        }

        public override Task ExecuteAsync()
        {
            _logger.Information("Hello from a BitMono plugin!");
            return Task.CompletedTask;
        }
    }

.. note::

    The container builds your protection by calling its **first** constructor and resolving each
    parameter. Take ``IBitMonoServiceProvider`` (as above) to reach BitMono's services. If a parameter
    cannot be resolved the protection is skipped and the error is logged - it never aborts the run.


.. _plugins-reference-the-host:

Reference BitMono, don't ship it
--------------------------------

Reference the BitMono assemblies with ``Private="false"`` so they are **not** copied next to your
plugin. Your plugin must bind to the *same* ``BitMono.API`` the host is running; if you ship your own
copy, your protection implements a *different* ``IProtection`` type and BitMono will ignore it (and warn
you in the log).

.. code-block:: xml

    <ItemGroup>
      <Reference Include="BitMono.API">
        <HintPath>path\to\BitMono.API.dll</HintPath>
        <Private>false</Private>
      </Reference>
      <Reference Include="BitMono.Core">
        <HintPath>path\to\BitMono.Core.dll</HintPath>
        <Private>false</Private>
      </Reference>
      <Reference Include="BitMono.Shared">
        <HintPath>path\to\BitMono.Shared.dll</HintPath>
        <Private>false</Private>
      </Reference>
    </ItemGroup>


External dependencies
---------------------

If your plugin uses external libraries (NuGet packages, your own helpers), ship those DLLs alongside the
plugin - either next to it or in a nested folder such as ``libs``. BitMono installs an
``AppDomain.AssemblyResolve`` handler that probes the plugin directories and loads dependencies on
demand, so they don't have to sit in BitMono's own folder.

A typical per-plugin layout::

    plugins/
      MyProtections/
        MyProtections.dll        <- your plugin
        libs/
          SomeNuGetPackage.dll   <- resolved automatically when needed


Enabling a plugin protection
----------------------------

Plugin protections are configured exactly like the built-in ones - by name (the class name, or the
``[ProtectionName("...")]`` value). Add them to ``protections.json``:

.. code-block:: json

    {
      "Protections": [
        {
          "Name": "HelloPlugin",
          "Enabled": true
        }
      ]
    }

or pass them on the command line:

.. code-block:: bash

    BitMono.CLI -f MyApp.dll -p HelloPlugin

Execution order follows the configuration order, the same as built-in protections. See
:doc:`obfuscation-execution-order` for details.
