Native AOT
==========

Yes, you can obfuscate a Native AOT app, but only if the obfuscation happens on the managed IL before
the AOT compiler runs. BitMono is an IL obfuscator, so it has to do its work *before* ``ilc`` (the
Native AOT compiler) turns your assembly into native code.

.. warning::

   Native AOT plus IL obfuscation is not officially supported by Microsoft. The .NET team has said
   there are no supported hooks for third-party code to modify the inputs to ``ilc``
   (`dotnet/runtime#121522 <https://github.com/dotnet/runtime/issues/121522>`_). It works in practice,
   but the exact MSBuild wiring can change between SDK versions (a known symptom on some .NET 9 SDKs is
   ``error: No entrypoint module`` during ``ilc``). Treat it as a community integration: pin your SDK
   and test the published native binary.

What changes with AOT
---------------------

A normal ``dotnet publish`` leaves a managed ``.dll`` in the output that BitMono rewrites directly.
With ``PublishAot``, ``dotnet publish`` instead:

1. compiles your C# to a managed IL assembly in ``obj/<config>/<tfm>/<rid>/`` (the *intermediate
   assembly*), then
2. runs ``ilc`` to turn that IL and its dependencies into one native executable.

There's no managed assembly left in the output to touch. So BitMono has to run between those two steps,
on the intermediate assembly, before ``ilc`` reads it.

Keep in mind the final binary is native code, so renaming buys you less than usual (there's no IL left
for a decompiler), and the protections that rely on IL/metadata or runtime tricks (``AntiDecompiler``,
``BitMono``, ``DotNetHook``, ``CallToCalli``, ...) are pointless or even harmful under AOT. For AOT
stick to lightweight IL-level transforms like ``StringsEncryption`` and ``FullRenamer``, and if you want
native-level protection put a native protector on top.

The easy way: the MSBuild NuGet integration
-------------------------------------------

:doc:`BitMono.Integration <msbuild-integration>` obfuscates the intermediate assembly right after
compilation (``AfterCompile``), which is before ``ilc`` consumes it. So for most projects AOT needs no
extra wiring, just add the package and publish:

.. code-block:: console

   dotnet publish -c Release -r win-x64 /p:PublishAot=true

If the app starts and runs, you're done. If ``ilc`` fails with ``No entrypoint module`` (or a similar
input error), you hit the unsupported-hook limitation above, try pinning to an SDK where it's known to
work for you (it has historically worked on .NET 8).

Doing it by hand
----------------

Not using the NuGet package? Hook a target that runs before ``IlcCompile`` and overwrite the
intermediate assembly in place. Don't move or rename it, keeping the same path is what stops the
``ilc`` input list from desyncing:

.. code-block:: xml

   <Target Name="BitMonoBeforeAot"
           BeforeTargets="IlcCompile"
           Condition="'$(PublishAot)' == 'true'">
     <Exec Command="BitMono.CLI -f &quot;$(IntermediateOutputPath)$(TargetName).dll&quot; -l &quot;$(IntermediateOutputPath)&quot; -o &quot;$(IntermediateOutputPath)bitmono&quot; -n &quot;$(TargetName).dll&quot;" />
     <Copy SourceFiles="$(IntermediateOutputPath)bitmono\$(TargetName).dll"
           DestinationFiles="$(IntermediateOutputPath)$(TargetName).dll"
           OverwriteReadOnlyFiles="true" />
   </Target>

Things to watch out for
-----------------------

- Keep renaming AOT-safe. Native AOT leans on metadata for some reflection and its own bookkeeping, so
  exclude anything read by reflection with ``criticals.json`` / ``[Obfuscation(Exclude = true)]`` and
  keep ``ReflectionMembersObfuscationExclude`` on.
- Members reached through a generic instantiation (e.g. ``Foo<int>().Bar()``) used to get renamed
  without their references being updated, which crashed AOT and JIT apps alike. That's fixed now,
  FullRenamer rewrites those references, just use a current BitMono build.
- ``No entrypoint module`` on ``ilc`` is the unsupported-hook limitation
  (`dotnet/runtime#121522 <https://github.com/dotnet/runtime/issues/121522>`_), not a bug in your
  config. It depends on the SDK version.

.NET MAUI
---------

- Android works with the normal flow, the code stays managed IL, so add the
  :doc:`BitMono.Integration <msbuild-integration>` package (or run the CLI) and build as usual.
- iOS is AOT-compiled (no JIT allowed), so the app head becomes a native arm64 image BitMono can't read,
  you'll see ``unsupported PE image architecture Arm64``. To protect an iOS app, obfuscate the IL before
  AOT: move the code you care about into class libraries and obfuscate those. They stay managed IL when
  BitMono rewrites them and get AOT-compiled afterwards, so the protection carries into the native
  output. Don't try to obfuscate the iOS app head itself.
