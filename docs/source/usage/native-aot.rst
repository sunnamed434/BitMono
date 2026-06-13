Native AOT
==========

Short answer: **yes, you can obfuscate a Native AOT app — but the obfuscation must happen on the
managed IL before the AOT compiler runs.** BitMono is an IL obfuscator, so it has to do its work
*before* ``ilc`` (the Native AOT compiler) turns your assembly into native code.

.. warning::

   Native AOT + IL obfuscation is **not officially supported by Microsoft**. The .NET team has
   stated there are *"no supported hooks that would allow third party code to modify inputs to
   ILC"* (`dotnet/runtime#121522 <https://github.com/dotnet/runtime/issues/121522>`_). It works in
   practice, but the exact MSBuild wiring can change between SDK versions (a known symptom on some
   .NET 9 SDKs is ``error: No entrypoint module`` during ``ilc``). Treat this as a community
   integration: pin your SDK and **test the published native binary**.

How Native AOT changes the picture
----------------------------------

A normal ``dotnet publish`` produces a managed ``.dll`` that BitMono rewrites directly. With
``PublishAot``, ``dotnet publish`` instead:

1. compiles your C# to a managed IL assembly in ``obj/<config>/<tfm>/<rid>/`` (the *intermediate
   assembly*), then
2. runs ``ilc`` to compile that IL **and its dependencies** into a single native executable.

There is no managed assembly left in the output to obfuscate. So BitMono must run between steps 1
and 2 — on the intermediate assembly, before ``ilc`` reads it.

.. note::

   Because the final binary is native code, **renaming buys less than usual** (there is no IL left
   for a decompiler), and several BitMono protections that exploit IL/metadata or runtime tricks
   (e.g. ``AntiDecompiler``, ``BitMono``, ``DotNetHook``, ``CallToCalli``) are meaningless or
   harmful under AOT. For AOT prefer lightweight, IL-level transforms (e.g. ``StringsEncryption``,
   ``FullRenamer``) and, for native-level protection, a native protector on top.

Recommended: the MSBuild NuGet integration
-------------------------------------------

:doc:`BitMono.Integration <msbuild-integration>` obfuscates the **intermediate** assembly right
after compilation (``AfterCompile``), which is *before* ``ilc`` consumes it. So for most projects
the AOT case needs no extra wiring — add the package and publish:

.. code-block:: console

   dotnet publish -c Release -r win-x64 /p:PublishAot=true

If the obfuscated app starts and runs, you are done. If ``ilc`` fails with ``No entrypoint
module`` (or a similar input error), you have hit the unsupported-hook limitation above — see the
caveats and try pinning to an SDK where it is known to work for you (it has historically worked on
.NET 8).

Manual target (running the CLI yourself)
----------------------------------------

If you don't use the NuGet package, hook a target that runs **before** ``IlcCompile`` and
overwrite the intermediate assembly **in place** (do not move/rename it — keeping the same path is
what avoids desyncing the ``ilc`` input list):

.. code-block:: xml

   <Target Name="BitMonoBeforeAot"
           BeforeTargets="IlcCompile"
           Condition="'$(PublishAot)' == 'true'">
     <Exec Command="BitMono.CLI -f &quot;$(IntermediateOutputPath)$(TargetName).dll&quot; -l &quot;$(IntermediateOutputPath)&quot; -o &quot;$(IntermediateOutputPath)bitmono&quot; -n &quot;$(TargetName).dll&quot;" />
     <Copy SourceFiles="$(IntermediateOutputPath)bitmono\$(TargetName).dll"
           DestinationFiles="$(IntermediateOutputPath)$(TargetName).dll"
           OverwriteReadOnlyFiles="true" />
   </Target>

Notes and known issues
----------------------

- **Keep renaming AOT-safe.** Native AOT relies on metadata for some reflection and for its own
  bookkeeping. Use ``criticals.json`` / ``[Obfuscation(Exclude = true)]`` to exclude anything read
  by reflection, and keep ``ReflectionMembersObfuscationExclude`` enabled.
- **Generic members.** A regression where members accessed through a generic instantiation (e.g.
  ``Foo<int>().Bar()``) were renamed but their references were not — which crashed AOT and JIT apps
  alike — is fixed (FullRenamer now rewrites those references). Use a current BitMono build.
- **"No entrypoint module" on ``ilc``** — this is the unsupported-hook limitation
  (`dotnet/runtime#121522 <https://github.com/dotnet/runtime/issues/121522>`_), not a bug in your
  obfuscation config. It is SDK-version dependent.
