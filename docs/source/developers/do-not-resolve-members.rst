Do Not Resolve Members
======================

When a protection runs, it works on a list of **members** (types, methods, fields, properties, events, …)
that BitMono already filtered for it, the list you get from ``Context.Parameters.Members``. Members that
shouldn't be touched (anything marked with ``[Obfuscation]`` or ``[DoNotResolve]``, and so on) are already
gone, and each one is handed to you as an AsmResolver ``IMetadataMember``.

.. code-block:: csharp

    public List<IMetadataMember> Members { get; }

``[DoNotResolve]`` lets *your* protection narrow that list further. Say you're writing a renamer and you
don't want to rename anything that's looked up by reflection, you'd never know to keep those names
otherwise. Add the attribute with the right flag and BitMono drops those members before they reach you:

.. code-block:: csharp

    [DoNotResolve(MemberInclusionFlags.Reflection)]
    public class MyRenamer : Protection

Combine flags with ``|`` to exclude several kinds at once:

.. code-block:: csharp

    [DoNotResolve(MemberInclusionFlags.SpecialRuntime | MemberInclusionFlags.Reflection)]
    public class MyRenamer : Protection

The flags
---------

.. list-table::
   :header-rows: 1
   :widths: 22 78

   * - Flag
     - Excludes
   * - ``SpecialRuntime``
     - Members the runtime treats as special: special-named methods/events, ``const`` (literal) fields, enum
       fields, and similar. Renaming these breaks the assembly, so almost every built-in protection sets
       this flag.
   * - ``Model``
     - Types and members carrying one of the *critical model attributes* you list in ``criticals.json``
       (``CriticalModelAttributes``, e.g. ``[Serializable]``). Keeps data shapes that serializers depend on
       intact. Honored only when ``UseCriticalModelAttributes`` is on.
   * - ``Reflection``
     - Members BitMono sees being accessed by reflection (``Type.GetMethod``, ``GetField``, ``GetProperty``,
       … by name). Renaming them would break the string lookup at runtime. Honored only when
       ``ReflectionMembersObfuscationExclude`` is on in ``obfuscation.json``.
   * - ``Baml``
     - Types (and their members) referenced by compiled WPF XAML (BAML), so renaming or namespace-stripping
       doesn't leave the XAML pointing at names that no longer exist and crash on load
       (`#212 <https://github.com/sunnamed434/BitMono/issues/212>`_).

If you don't add ``[DoNotResolve]`` at all, your protection gets every member in the sorted list.

Iterate the sorted list, not the module
----------------------------------------

.. warning::

    ``[DoNotResolve]`` only filters ``Context.Parameters.Members``. It does **not** touch
    ``Context.Module``, walking the module directly bypasses the sorting entirely.

So this is wrong, it ignores everything BitMono sorted for you:

.. code-block:: csharp

    foreach (var type in Context.Module.GetAllTypes()) // don't
    {
    }

Iterate ``Context.Parameters.Members`` instead and pull out the member kinds you need with ``OfType<>``:

.. code-block:: csharp

    // types
    foreach (var type in Context.Parameters.Members.OfType<TypeDefinition>())
    {
    }

    // methods
    foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
    {
    }

One catch: the *member list* is sorted, but the members hanging off a type aren't. If you grab a type and
then read ``type.Methods``, those methods skipped the filter. Always go through the list for each kind you
touch, rather than reaching into ``type.Methods``, ``type.Fields``, and so on.

The analyzer has your back
--------------------------

You don't have to keep all this in your head. BitMono ships a small Roslyn analyzer (``BITM0001``) that
spots ``Context.Module.GetAllTypes()`` inside a protection and nudges you toward
``Context.Parameters.Members``, with a one-click fix to swap it over. It rides along with the
``BitMono.Core`` package, so it lights up in :doc:`plugin <plugins>` projects too, not just the built-in
protections. If you really do mean to walk the raw module (collecting references and the like), hit
``Alt+Enter`` / ``Ctrl+.`` on the warning and suppress it - that's a normal thing to do, not a hack.
