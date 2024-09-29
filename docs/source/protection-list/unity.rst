Unity Protections
=================

This section lists various protections compatible with the `Unity Engine <https://unity.com/>`_. 

Unity Runtime is typically based on the .NET Framework 4.*, depending on the Unity version in use. However, it is primarily executed on `Mono <https://github.com/mono/mono>`_, with Unity using its `own fork of Mono <https://github.com/Unity-Technologies/mono>`_ specifically for Unity Runtime. 

You can also refer to this list for Mono runtime protections, but be aware that while Unity's Mono fork is similar to the standard Mono runtime, there may be differences that could impact compatibility.

Protection List
---------------

- **StringsEncryption**  
  Can significantly slow down your application.

- **BitDotNet**  
  Do not use with Unity versions higher than 2020.*. and instead use BitDecompiler

- **BitMethodDotnet**

- **DotNetHook**

- **CallToCalli**

- **ObjectReturnType**  
  Unstable.

- **NoNamespaces**  
  May cause issues if you rely heavily on Reflection.

- **FullRenamer**  
  May cause issues if you rely heavily on Reflection.

- **AntiDebugBreakpoints**  
  Unstable.

- **AntiDecompiler**

- **BitDecompiler**

- **BitDateTimeStamp**

- **BitMono**

- **BillionNops**

- **AntiDe4dot**

- **AntiILdasm**

Additional Considerations
-------------------------

- Some of these protections may not be suitable for all Unity versions or Mono runtimes.
- When using optional protections like `NoNamespaces` or `FullRenamer`, ensure to test thoroughly if your application uses extensive Reflection, as they might break functionality.