Obfuscation Engine Execution Order
==================================

BitMono uses its own obfuscation execution order which is good to be known, and it reminds ConfuserEx a lot, if you're familiar with it you can be easier with it.

1. Output Loaded Module Info
2. Output Information about BitMono (example, is it intended for .NET Core or Mono or .NET Framework, etc.) and running OS, etc.
3. Output Compatibility Issues in case of module is built for .NET Framework, but BitMono is running on .NET Core, or vice versa.
4. Sort Protections
5. Basic output information about Protections
6. Elapsed time counter
7. Resolve References
8. Expand Macros
10. Run Protection, PipelineProtection and child pipeline protections


.. code-block:: csharp

	public class StandardProtection : Protection

	public class Pipeline : PipelineProtection


11. Optimize Macros
12. [ObfuscationAttribute] cleanup
13. Create PE Image
14. Write Module
15. Run Packers


.. code-block:: csharp

	public class Packer : PackerProtection


16. Output Elapsed Time since obfuscation 