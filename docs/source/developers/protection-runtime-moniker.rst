Protection Runtime Moniker
==========================

You have protection that works only with specific runtime and you want to let it know to the user.
By default BitMono provides an opportunity to talk with the users, to warn them, like be careful, this protection working only with ``Mono``.

.. code-block:: csharp

	[UsedImplicitly]
	[RuntimeMonikerMono] // Add this Attribute which says this protections works only with Mono Runtime
	public class MonoPacker : Packer


If you will check what's going on under the hood, you will see that it simply specifies ``Mono`` inside of the constructor.


.. code-block:: csharp

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class RuntimeMonikerMonoAttribute : RuntimeMonikerAttribute
	{
		// public const string Mono = "Mono";
	    public RuntimeMonikerMonoAttribute() : base(KnownRuntimeMonikers.Mono)
	    {
	    }
	}


If you will go further, you can see what's actually going on here, it says ``Intended for Mono runtime``, so that means to the endpoint user it will output this information and warn they.


.. code-block:: csharp

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public abstract class RuntimeMonikerAttribute : Attribute
	{
	    protected RuntimeMonikerAttribute(string name)
	    {
	        Name = name;
	    }
	
	    public string Name { get; }
	
	    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
	    public virtual string GetMessage()
	    {
	        return $"Intended for {Name} runtime";
	    }
	}


Let's create your own Rust Runtime Moniker Attribute.


.. code-block:: csharp

	public class RuntimeMonikerRustAttribute : RuntimeMonikerAttribute
	{
	    public RuntimeMonikerRustAttribute() : base("Rust")
	    {
	    }
	}



Specify Rust Runtime Moniker Attribute.


.. code-block:: csharp

	[UsedImplicitly]
	[RuntimeMonikerRust] // Add this Attribute which says this protections works only with Rust Runtime
	public class RustPacker : Packer // or instead use Protection or PipelineProtection



After that user need to use the ``RustPacker`` and they will receive an message that the ``RustPacker`` "is intended for Rust runtime".



.. note::

	You don't need to make any services registration or instance creation for the ``RuntimeMonikerRust`` due to it will get the attribute automatically behind the hood using Reflection, and Protections Info Output in the Console/GUI (whatever is used, user will get a message notification about that). So, you don't need to care about ``RuntimeMonikerRust`` anymore, simply add it on top of the feature and have fun!