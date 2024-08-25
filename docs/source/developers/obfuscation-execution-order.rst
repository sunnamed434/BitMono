Obfuscation Engine Execution Order
==================================

BitMono uses its own obfuscation execution order which is good to be known, and it reminds ConfuserEx a lot, if you're familiar with it you can be easier with it.

1. Output Loaded Module Info
2. Output Information about BitMono (example, is it intended for .NET Core or Mono or .NET Framework, etc.) and running OS, etc.
3. Output Compatibility Issues in case of module is built for .NET Framework, but BitMono is running on .NET Core, or vice versa.
4. Sort Protections
5. Information about Protections
6. Configuration for Native Code
7. Elapsed time counter
8. Resolve References
9. Expand Macros
11. Run Protection, PipelineProtection and child pipeline protections


.. code-block:: csharp

	public class StandardProtection : Protection

	public class Pipeline : PipelineProtection


12. Optimize Macros
13. [ObfuscationAttribute] cleanup
14 Create PE Image
15. Write Module
16. Run Packers


.. code-block:: csharp

	public class Packer : PackerProtection


17. Output Elapsed Time since obfuscation 