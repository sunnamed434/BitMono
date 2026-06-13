Plugins
=======

BitMono can load **plugins**, external assemblies that add your own protections, without rebuilding
BitMono. Drop a plugin DLL into the plugins directory and BitMono finds the protections inside it and
runs them exactly like the built-in ones. (`#227 <https://github.com/sunnamed434/BitMono/issues/227>`_)

.. warning::

    Plugins run inside the BitMono process with full trust, .NET can't sandbox them. Only drop in
    plugins from sources you trust, same as you would with any other executable.


Where plugins live
------------------

By default BitMono scans a ``plugins`` directory next to the BitMono executable. You can rename it (or
point it at an absolute path) with ``PluginsDirectoryName`` in ``obfuscation.json``:

.. code-block:: json

    {
      "PluginsDirectoryName": "plugins"
    }

or override it for a single run from the CLI with ``--plugins``:

.. code-block:: bash

    BitMono.CLI -f MyApp.dll --plugins "C:\path\to\my-plugins" -p HelloPlugin

Two layouts work:

- **Flat**, drop the DLL straight in: ``plugins/MyProtections.dll``.
- **Per-plugin folder**, give each plugin its own folder: ``plugins/MyProtections/MyProtections.dll``.
  Put any extra dependencies in a nested folder (say ``plugins/MyProtections/libs/``), they're resolved
  on demand and aren't treated as plugins themselves.

If the directory doesn't exist, BitMono just skips plugin loading.


Writing a plugin
----------------

A plugin is an ordinary class library that references ``BitMono.API`` (and usually ``BitMono.Core`` for
the ``Protection`` base class) and exposes one or more public protections. Writing the protection itself
is the same as writing a :doc:`built-in one <first-protection>`:

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

    The container builds your protection through its **first** constructor, resolving each parameter.
    Take ``IBitMonoServiceProvider`` (like above) to reach BitMono's services. If a parameter can't be
    resolved the protection is skipped and the error is logged, it never aborts the run.


.. _plugins-reference-the-host:

Reference BitMono, don't ship it
--------------------------------

Reference BitMono from NuGet and mark it ``ExcludeAssets="runtime"`` so its assemblies are **not** copied
into your build output (BitMono itself provides them at runtime). Your plugin has to bind to the *same*
``BitMono.API`` the host is running. Ship your own copy and your protection implements a *different*
``IProtection`` type, so BitMono ignores it (and warns you in the log).

``BitMono.Core`` pulls in ``BitMono.API``, ``BitMono.Shared`` and ``AsmResolver`` for you, and
``ExcludeAssets="runtime"`` keeps the whole graph out of your output, so a successful build leaves just
your own plugin DLL behind.

.. code-block:: xml

    <ItemGroup>
      <!-- Use the version that matches the BitMono you run the plugin against. -->
      <PackageReference Include="BitMono.Core" Version="0.40.1">
        <ExcludeAssets>runtime</ExcludeAssets>
      </PackageReference>
    </ItemGroup>


Versioning and compatibility
----------------------------

There's no special "plugin version" attribute - your plugin's own assembly version is its version, and
BitMono prints it when it loads it (``Loaded plugin: MyPlugin v1.2.0.0``).

What actually matters is the ``BitMono.API`` version you build against (``BitMono.Core`` pulls it in). It's
the contract, and it follows `semver <https://semver.org>`_: a new minor only adds things, a new major can
break ``IProtection``/``Protection``. If a plugin is built against a *newer* ``BitMono.API`` than the
BitMono you drop it into, it'd probably call something that isn't there - so BitMono skips it and logs a
warning telling you to rebuild against the version you're running. Same or older builds load fine.

So pin ``BitMono.Core`` to the version you ship against and bump it when you move to a newer BitMono.


External dependencies
---------------------

If your plugin uses external libraries (other NuGet packages, your own helpers), reference them
**normally** (without ``ExcludeAssets`` - that switch is only for BitMono itself) so they're copied next
to your plugin, then ship those DLLs next to the plugin or in a nested folder like ``libs``. BitMono
installs an ``AppDomain.AssemblyResolve`` handler that probes the plugin directories and loads
dependencies on demand, so they don't have to sit in BitMono's own folder.

A typical per-plugin layout::

    plugins/
      MyProtections/
        MyProtections.dll        <- your plugin
        libs/
          SomeNuGetPackage.dll   <- resolved automatically when needed


Enabling a plugin protection
----------------------------

Plugin protections are configured exactly like the built-in ones, by name (the class name, or the
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

Execution order follows the configuration order, same as built-in protections. See
:doc:`obfuscation-execution-order` for details.
