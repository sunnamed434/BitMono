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

Encrypting the IL2CPP output (global-metadata.dat)
--------------------------------------------------

Everything above is about the *managed* side, renaming and string encryption that land inside
``global-metadata.dat`` because BitMono runs before ``il2cpp.exe``. That already cloaks your names, but the
``global-metadata.dat`` file itself is still a normal, parseable file, so tools like Il2CppDumper and Cpp2IL
happily read its structure straight off disk.

The IL2CPP metadata encryption protection (issue #276) closes that. When you turn it on, BitMono encrypts
``global-metadata.dat`` in the built player so static dumpers can't parse it at all, and a tiny native
decryptor that BitMono compiles into the IL2CPP binary restores it in memory at startup. The game boots
exactly as before; the dumpers just see noise.

It's a separate, independent layer from the managed obfuscation, you can use either or both. Turn it on in
Unity from the **BitMonoConfig** asset: tick **Encrypt IL2CPP Metadata**. Works on Windows x64 and Android
(arm64/x86_64).

.. code-block:: text

    BitMonoConfig
      [x] Enable Obfuscation          (managed renaming/strings, runs before il2cpp.exe)
      [x] Encrypt IL2CPP Metadata     (encrypts global-metadata.dat, decrypts in GameAssembly.dll)

Under the hood it's two halves that share a key BitMono generates fresh for every build:

- **Offline:** after the player is built, BitMono runs ``BitMono.CLI --encrypt-metadata global-metadata.dat
  --metadata-key <hex>``. That XXTEA-encrypts the whole file behind a small header and self-checks the
  round-trip. You can run it by hand for CI builds too (omit ``--metadata-key`` to use the fixed dev key).
- **Runtime:** the source plugin ``global_metadata_decrypt.cpp`` (shipped in the Unity package) is compiled
  into the IL2CPP binary - ``GameAssembly.dll`` on Windows, ``libil2cpp.so`` on Android - and hooks the file
  read of ``global-metadata.dat`` (``CreateFileW`` on Windows, ``open`` on Android) to hand IL2CPP the
  decrypted bytes. It's only compiled in when you turn the feature on, so a plain build ships no hook at all.

.. note::

   This stops **static** dumping, the shipped ``global-metadata.dat`` is unreadable, so anything that parses
   the file off disk is dead in the water. It does **not** stop a **memory** dumper that reads the already
   decrypted bytes out of the running process; nothing that ships the key in the binary can. Treat it as one
   more wall on top of the managed renaming, not a magic bullet. The key ships inside the IL2CPP binary so
   it's obfuscation strength, not a secret - but it's random per build, so cracking one game's key doesn't
   unlock every other BitMono game.

Validated end to end on real Unity 6000.2 IL2CPP builds (metadata version 31): Windows x64, and Android
x86_64 on an emulator (arm64 shares the same 64-bit code path). The encryption is whole-file, so it doesn't
care about the per-version metadata layout; only the decryptor's file hook is platform-specific.
