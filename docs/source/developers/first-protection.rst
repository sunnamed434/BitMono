Creating your first Protection
==============================

.. warning::

    BitMono provides a lot of examples in source code with existing protection and maximum functional usage, you can find them in BitMono.Protections project.


Create your protection in the ``BitMono.Protections`` namespace.

- The Dependency Injection (DI) container will automatically register your protections.
- The BitMono Obfuscation Engine will invoke your protection based on its type and the order specified in the configuration file.
- The order of execution is determined by the position of each protection in the ``protections.json`` file within the configuration file. For example, AntiILdasm is executed first (because this protection is first in configuration) and Packers always run last after all protections, even if you set this ``Packer`` protection as a first one in configuration it will anyway gonna be called last.


.. code-block:: csharp
	
	public class StandardProtection : Protection
	{
	    // Inject services right here
	    public StandardProtection(IServiceProvider serviceProvider) : base(serviceProvider)
	    {
	    }
	
	    public override Task ExecuteAsync()
	    {
	        // All protection are intended to be async, so you can simply await your things, or if you don't have,
	        // then use Task.CompletedTask
	        return Task.CompletedTask;
	    }
	}