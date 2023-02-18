Exclude member from being obfuscated
====================================

Let's say you have something specific that you don't want to protect, in this case you can add an ``[ObfuscationAttribute]`` and specify there protection name.

.. code-block:: csharp

	using System;
	using System.Xml.Serialization;
	using System.Runtime.CompilerServices;
	using Newtonsoft.Json;
	
	[assembly: Obfuscation(Feature = "DotNetHook")] // Ignoring whole assembly or in the AssemblyInfo.cs (sometimes it would not exist in your project)
	[assembly: Obfuscation(Feature = "NoNamespaces")] // Ignoring whole assembly or in the AssemblyInfo.cs (sometimes it would not exist in your project)
	namespace Project
	{
	}
	
	// Enough to add attribute to whole class type to ignore obfuscation of concrete protection
	[Obfuscation(Feature = "FullRenamer")] // Add this attribute to ignore renaming of method
	class MyClass
	{
	    [MethodImpl(MethodImplOptions.NoInlining)] // Add this attribute to ignore renaming of method
	    void MyMethod()
	    {
	        // potential critical code used to be here
	    }
	
	    // Add this attribute to ignore renaming of method
	    [Obfuscation(Feature = "ObjectReturnType")]  // This attribute is won`t work in this case, because 'MyClass' has attribute with the same feature and `ApplyToMembers` set to true
	    [Obfuscation(Feature = "CallToCalli")] 
	    void MyAnotherMethod()
	    {
	        // potential critical code used to be here
	    }
	}
	
	// Obfuscation will be still applied to the inherted type
	[Obfuscation(Feature = "BitDotNet")] // Adding ObfuscationAttribute once is enough to ignore members
	public interface IInterface
	{
	    string Text { get; }
	
	    [Obfuscation(Feature = "FullRenamer")]
	    Task DoSomethingAsync();
	}
	
	// IInterface obfuscation attribute doesn`t work in implementation
	[Obfuscation(Feature = "FullRenamer")] // Adding ObfuscationAttribute once is enough to ignore members
	public class InterfaceImplementation : IInterface
	{
	    public string Text { get; }
	
	    [Obfuscation(Feature = "BitMethodDotnet")]
	    public Task DoSomethingAsync()
	    {
	        return Task.CompletedTask;
	    }
	}
	
	
	[Serializable] // Marking as serializable attribute is enough to ignore everything in this model
	class ProductModel
	{
	    [XmlAttribute("Product Name")] // Marking as Xml attribute
	    string Name { get; set; }
	    [JsonProperty("Product Description")] // Or marking as Json Property
	    string Description { get; set; }
	    [XmlAttribute("Product Price")]
	    double Price { get; set; }
	}