IL2CPP Compatibility
====================

Some protections can't survive a Unity **IL2CPP** build. Mark those with ``[IL2CPPIncompatible]`` and
BitMono skips them automatically whenever it runs in IL2CPP mode, while still running them normally on
Mono and standalone .NET.

.. code-block:: csharp

    [IL2CPPIncompatible("Emits calli, which IL2CPP's AOT compiler does not support")]
    public class CallToCalli : Protection

The reason you pass is shown to the user, so they understand why it was skipped:

.. code-block:: text

    [IL2CPP] CallToCalli - Emits calli, which IL2CPP's AOT compiler does not support

Like the :doc:`runtime moniker <protection-runtime-moniker>`, the attribute is read by reflection, you
don't register anything. Add it on top of the protection (built-in or a :doc:`plugin <plugins>`) and
you're done. IL2CPP mode is set automatically by the Unity integration when the scripting backend is
IL2CPP, or by hand with the CLI ``--il2cpp`` flag / ``"IL2CPP": true`` in ``obfuscation.json``.

Should I mark my protection?
----------------------------

On an IL2CPP build the managed ``Assembly-CSharp.dll`` isn't shipped, ``il2cpp.exe`` consumes it and
converts it to C++ (``GameAssembly.dll``). BitMono obfuscates *before* that conversion, so where your
protection's output ends up decides this:

- **Don't mark it** if it only does managed metadata/IL edits ``il2cpp.exe`` can parse, renaming,
  clearing namespaces, swapping ``ldstr`` for a managed decryptor call. That effect carries through into
  ``global-metadata.dat``, which is exactly what you want.
- **Mark it** if it emits native code, ``calli``, packs the PE, or otherwise produces output
  ``il2cpp.exe`` can't handle. It would either break the conversion or only touch the managed PE that
  IL2CPP throws away.

.. note::

   Protections marked with :doc:`native-code` (``[ConfigureForNativeCode]``) are treated as
   IL2CPP-incompatible automatically, native method bodies can never become C++, so you don't need to add
   ``[IL2CPPIncompatible]`` on top of them as well.

See :doc:`../protection-list/unity` for which built-in protections run on IL2CPP and which are skipped.
