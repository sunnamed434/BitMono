Protection Runtime Moniker
==========================

Some protections only make sense on one runtime. If yours is built for Mono and would do nothing (or
break things) elsewhere, say so with a **runtime moniker** attribute. BitMono then warns the user when
they enable it, something like *"Intended for Mono runtime"*, so nobody's surprised.

.. code-block:: csharp

    [RuntimeMonikerMono] // this protection is meant for the Mono runtime
    public class MonoPacker : Packer

That's all you have to do. BitMono reads the attribute by reflection and shows the message in the
protections list (CLI or GUI), no registration, no extra wiring.

Built-in monikers
-----------------

- ``[RuntimeMonikerMono]`` — Mono
- ``[RuntimeMonikerNETCore]`` — .NET / .NET Core
- ``[RuntimeMonikerNETFramework]`` — .NET Framework

No moniker at all means the protection is assumed to work everywhere.

Your own moniker
----------------

Need a runtime that isn't in the list? Subclass ``RuntimeMonikerAttribute`` and pass its name, the name
is what shows up in the warning message:

.. code-block:: csharp

    public class RuntimeMonikerRustAttribute : RuntimeMonikerAttribute
    {
        public RuntimeMonikerRustAttribute() : base("Rust")
        {
        }
    }

Then use it like any built-in one:

.. code-block:: csharp

    [RuntimeMonikerRust] // works on Protection, PipelineProtection or Packer
    public class RustPacker : Packer

Now anyone enabling ``RustPacker`` gets told it's *"intended for Rust runtime"*.

.. note::

    A moniker only *informs* the user, it doesn't stop the protection from running. If you need a
    protection skipped automatically on a given build, that's a different mechanism, see
    :doc:`il2cpp-compatibility`.
