#pragma warning disable CS8602
namespace BitMono.Core.Protecting.Injection;

public class MscorlibInjector
{
    public FieldDefinition InjectCompilerGeneratedArray(ModuleDefinition module, TypeDefinition type, byte[] data, string name)
    {
        var importer = module.DefaultImporter;
        var valueType = importer.ImportType(typeof(ValueType));
        var classWithLayout = new TypeDefinition(null, "<>c", TypeAttributes.NestedAssembly | TypeAttributes.Sealed | TypeAttributes.ExplicitLayout, valueType)
        {
            ClassLayout = new ClassLayout(0, (uint)data.Length),
        };
        InjectCompilerGeneratedAttribute(module, classWithLayout);
        type.NestedTypes.Add(classWithLayout);

        var fieldWithRVA = new FieldDefinition("<>c", FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.HasFieldRva, new FieldSignature(classWithLayout.ToTypeSignature()));
        fieldWithRVA.FieldRva = new DataSegment(data);
        classWithLayout.Fields.Add(fieldWithRVA);

        var systemByte = importer.ImportType(module.CorLibTypeFactory.Byte.ToTypeDefOrRef());
        var fieldInjectedArray = new FieldDefinition(name, FieldAttributes.Public | FieldAttributes.Static, new FieldSignature(systemByte.MakeSzArrayType()));
        classWithLayout.Fields.Add(fieldInjectedArray);

        var initializeArrayMethod = importer.ImportMethod(typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.InitializeArray), new Type[]
        {
            typeof(Array),
            typeof(RuntimeFieldHandle)
        }));

        var cctor = classWithLayout.GetOrCreateStaticConstructor();
        var instructions = cctor.CilMethodBody.Instructions;
        instructions.InsertRange(0, new CilInstruction[]
        {
            new CilInstruction(CilOpCodes.Ldc_I4, data.Length),
            new CilInstruction(CilOpCodes.Newarr, systemByte),
            new CilInstruction(CilOpCodes.Dup),
            new CilInstruction(CilOpCodes.Ldtoken, fieldWithRVA),
            new CilInstruction(CilOpCodes.Call, initializeArrayMethod),
            new CilInstruction(CilOpCodes.Stsfld, fieldInjectedArray),
        });
        return fieldInjectedArray;
    }
    public TypeDefinition InjectCompilerGeneratedValueType(ModuleDefinition module, TypeDefinition type, string? name = null)
    {
        var result = CreateCompilerGeneratedValueType(module, name);
        type.NestedTypes.Add(result);
        return result;
    }
    public CustomAttribute InjectCompilerGeneratedAttribute(ModuleDefinition module, IHasCustomAttribute @in)
    {
        var attribute = CreateCompilerGeneratedAttribute(module);
        @in.CustomAttributes.Add(attribute);
        return attribute;
    }
    public CustomAttribute InjectAttribute(ModuleDefinition module, string @namespace, string name, IHasCustomAttribute @in)
    {
        var attribute = CreateAttribute(module, @namespace, name);
        @in.CustomAttributes.Add(attribute);
        return attribute;
    }
    public TypeDefinition CreateCompilerGeneratedType(ModuleDefinition module, string? name = null)
    {
        var @object = module.CorLibTypeFactory.Object.ToTypeDefOrRef();
        var invisibleType = new TypeDefinition(null, name ?? "<PrivateImplementationDetails>", TypeAttributes.Public, @object);
        InjectCompilerGeneratedAttribute(module, invisibleType);
        return invisibleType;
    }
    public TypeDefinition CreateCompilerGeneratedValueType(ModuleDefinition module, string? name = null)
    {
        var valueType = module.DefaultImporter.ImportType(typeof(ValueType));
        var invisibleValueType = new TypeDefinition(null, name ?? "<PrivateImplementationDetails>", TypeAttributes.NestedPublic, valueType);
        InjectCompilerGeneratedAttribute(module, invisibleValueType);
        return invisibleValueType;
    }
    public CustomAttribute CreateCompilerGeneratedAttribute(ModuleDefinition module)
    {
        var attribute = CreateAttribute(module, typeof(CompilerGeneratedAttribute).Namespace, nameof(CompilerGeneratedAttribute));
        return attribute;
    }
    public CustomAttribute CreateAttributeWithContent(ModuleDefinition module, string @namespace, string name, string content)
    {
        var factory = module.CorLibTypeFactory;
        var ctor = factory.CorLibScope
            .CreateTypeReference(@namespace, name)
            .CreateMemberReference(".ctor", MethodSignature.CreateInstance(factory.Void, factory.String)
            .ImportWith(module.DefaultImporter));

        var attribute = new CustomAttribute(ctor);
        attribute.Signature.FixedArguments.Add(new CustomAttributeArgument(factory.String, content));
        return attribute;
    }
    public CustomAttribute CreateAttribute(ModuleDefinition module, string @namespace, string name)
    {
        var factory = module.CorLibTypeFactory;
        var ctor = factory.CorLibScope
            .CreateTypeReference(@namespace, name)
            .CreateMemberReference(".ctor", MethodSignature.CreateInstance(factory.Void))
            .ImportWith(module.DefaultImporter);

        return new CustomAttribute(ctor);
    }
}