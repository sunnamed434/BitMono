Creating your first Protection
==============================

.. warning::

    BitMono provides a lot of examples in source code with existing protection and maximum functional usage, you can find them in BitMono.Protections project.


Create your protection in the ``BitMono.Protections`` namespace.

- The Dependency Injection (DI) container registers your protections automatically.
- The BitMono Obfuscation Engine invokes your protection based on its type and its position in the
  configuration file.
- Execution order follows the order in ``protections.json``, e.g. ``AntiILdasm`` runs first because it's
  listed first. The exception is Packers: they always run last, no matter where you place them in the
  list.


.. code-block:: csharp
	
	public class StandardProtection : Protection
	{
	    // Inject services right here
	    public StandardProtection(IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
	    {
	    }

	    public override Task ExecuteAsync()
	    {
	        // All protection are intended to be async, so you can simply await your things, or if you don't have,
	        // then use Task.CompletedTask
	        return Task.CompletedTask;
	    }
	}