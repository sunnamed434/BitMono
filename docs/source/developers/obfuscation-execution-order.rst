Obfuscation Engine Execution Order
==================================

BitMono runs through a fixed order every time it obfuscates, worth knowing if you're writing your own
protection. If you've used ConfuserEx, a lot of this will feel familiar.

1. Output Loaded Module Info
2. Output Information about BitMono (example, is it intended for .NET Core or Mono or .NET Framework, etc.) and running OS, etc.
3. Output Compatibility Issues in case of module is built for .NET Framework, but BitMono is running on .NET Core, or vice versa.
4. Sort Protections
5. Information about Protections
6. Configuration for Native Code
7. Elapsed time counter
8. Resolve References
9. Expand Macros
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
17. Output Tips (short, helpful hints printed after a run; can be turned off with the ``Tips`` setting in ``obfuscation.json``) 