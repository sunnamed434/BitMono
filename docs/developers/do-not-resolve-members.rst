Do Not Resolve Members
======================

For comfort BitMono provides an API which able to do not pass specfic members inside of the protection for easier understanding and abstraction let's call ``members`` as - types/methods/fields/properties, etc.


.. code-block:: csharp


	public override Task ExecuteAsync(ProtectionParameters parameters)


Everything which is passed inside of the ``parameters`` is all members which were found inside of the module and sorted by BitMono (skipped members with [ObfuscationAttributes] and not only), and passed using ``IMetadataMember`` AsmResolver's APIs.


.. code-block:: csharp


    public List<IMetadataMember> Members { get; }


That's mean if you will specify attribute on your protection and say I want all members but please, I'm writing my own renamer and I don't want to get members which were used somewhere by reflection, right?
Add attribute ``[DoNotResolve(MemberInclusionFlags.Reflection)]`` with ``MemberInclusionFlags.Reflection`` parameter.


.. code-block:: csharp


	[UsedImplicitly] // This is not intentional, but suppresses warnings by ReSharper
	[DoNotResolve(MemberInclusionFlags.Reflection)]
	public class MagicProtection : Protection



You can specify multiple inclusion flags:


.. code-block:: csharp
	[UsedImplicitly]
	[DoNotResolve(MemberInclusionFlags.SpecialRuntime | MemberInclusionFlags.Reflection)]
	public class MagicProtection : Protection


.. warning::

    Be careful, because ``Module`` doesn't affected by ``DoNotResolveAttribute``.


THIS IS TOTALLY BAD AND WRONG! Sorting doesn't affects to the actual Module.


.. code-block:: csharp


	public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        foreach (var type in Context.Module.GetAllTypes())
        {

        }
    }



Instead highly recommend to use this:


.. code-block:: csharp


	public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        foreach (var type in parameters.Members.OfType<TypeDefinition>())
        {

        }
    }


This is also was wrong because if you will try to get access to the ``type.Methods``, etc, methods are not sorted, use specificly what you need, for example:
- Need access to the types and methods? Then do this:


.. code-block:: csharp


	public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        foreach (var type in parameters.Members.OfType<TypeDefinition>())
        {

        }
        foreach (var type in parameters.Members.OfType<MethodDefinition>())
        {

        }
    }


- Need access to the methods? Then just iterrate through the methods:


.. code-block:: csharp

	public override Task ExecuteAsync(ProtectionParameters parameters)
    {
       
        foreach (var type in parameters.Members.OfType<MethodDefinition>())
        {

        }
    }