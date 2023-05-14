Obfuscation Engine Execution Order
==================================

BitMono uses its own obfuscation execution order which is good to be known, and it reminds ConfuserEx a lot, if you're familiar with it you can be easier with it.

1. Output Loaded Module Info
2. Sort Protections
3. Basic output information about Protections
4. Elapsed time counter
5. Output Information of Running Framework
6. Resolve References
7. Expand Macros
8. Run Protection, PipelineProtection and child pipeline protections


.. code-block:: csharp

	public class StandardProtection : Protection

	public class Pipeline : PipelineProtection


9. Optimize Macros
10. [ObfuscationAttribute] cleanup
11. Create PE Image
12. Write Module
13. Run Packers


.. code-block:: csharp

	public class Packer : PackerProtection


14. Output Elapsed Time since obfuscation 