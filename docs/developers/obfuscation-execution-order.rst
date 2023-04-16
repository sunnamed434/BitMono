Obfuscation Engine Execution Order
==================================

BitMono uses its own obfuscation execution order which is good to be known, and it reminds ConfuserEx a lot, if you're familiar with it you can be easier with it.

1. Basic output information about Protections
2. Elapsed time counter
3. Output Information of Running Framework
4. Resolve References
5. Expand Macros
6. Run Protection, PipelineProtection and child pipeline protections


.. code-block:: csharp

	public class StandardProtection : Protection

	public class Pipeline : PipelineProtection


5. Optimize Macros
6. [ObfuscationAttribute] cleanup
7. Create PE Image
8. Output PE Image Build Errors
9. Write Module
10. Run Packers


.. code-block:: csharp

	public class Packer : PackerProtection


11. Output Elapsed Time since obfuscation 