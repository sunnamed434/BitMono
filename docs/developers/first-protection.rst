Creating your first Protection
==============================

.. warning::

    BitMono provides a lot of examples in source code with existing protection and maximum functional usage, you can find them in BitMono.Protections project.


Always create your protection ONLY in BitMono.Protections, DI (dependency injection) container will catch all of your protections automatically and Obfuscation Engine of BitMono will automatically call your protection depending on which protection is it.


.. code-block:: csharp
	
	// Mark the Protection as [UsedImplicitly] because for JetBrains Rider or ReSharper users protection will look kinda is not used,
	// and other developers might delete it as an unnecessary class in the project,
	// because protections are instantiated via DI container, so, its invisible for JetBrains Rider and ReSharper,
	// the goal is remove weird warning,
	// In simple words, saying to the rider: 
	// "It's ok, protection created somewhere else but you can't see it,
	// please don't worry, and remove your warnings"
	[UsedImplicitly]
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