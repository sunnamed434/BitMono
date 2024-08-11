Obfuscation Engine Execution Order
==================================

BitMono uses its own obfuscation execution order which is good to be known, and it reminds ConfuserEx a lot, if you're familiar with it you can be easier with it.

1. Output Loaded Module Info
2. Output Information about BitMono (example, is it intended for .NET Core or Mono or .NET Framework, etc.) and running OS, etc.
3. Sort Protections
4. Basic output information about Protections
5. Elapsed time counter
7. Resolve References
8. Expand Macros
9. Run Protection, PipelineProtection and child pipeline protections


.. code-block:: csharp

	public class StandardProtection : Protection

	public class Pipeline : PipelineProtection


10. Optimize Macros
11. [ObfuscationAttribute] cleanup
12. Create PE Image
13. Write Module
14. Run Packers


.. code-block:: csharp

	public class Packer : PackerProtection


15. Output Elapsed Time since obfuscation 