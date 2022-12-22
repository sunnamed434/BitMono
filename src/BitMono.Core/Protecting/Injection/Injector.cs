namespace BitMono.Core.Protecting.Injection;

public class Injector : IInjector
{
    public FieldDefinition InjectInvisibleArray(ModuleDefinition module, TypeDefinition type, byte[] data, string name)
    {
        var valueType = module.DefaultImporter.ImportType(typeof(ValueType));
        var classWithLayout = new TypeDefinition(null, "<>c", TypeAttributes.Sealed | TypeAttributes.ExplicitLayout, module.DefaultImporter.ImportType(valueType))
        {
            ClassLayout = new ClassLayout(0, (uint)data.Length),
        };
        var compilerGeneratedAttribute = InjectCompilerGeneratedAttribute(module);
        classWithLayout.CustomAttributes.Add(compilerGeneratedAttribute);
        type.NestedTypes.Add(classWithLayout);

        var fieldWithRVA = new FieldDefinition("<>c", FieldAttributes.Assembly | FieldAttributes.Static | FieldAttributes.HasFieldRva | FieldAttributes.InitOnly, new FieldSignature(classWithLayout.ToTypeSignature()));
        fieldWithRVA.FieldRva = new DataSegment(data);
        classWithLayout.Fields.Add(fieldWithRVA);

        var byteArray = module.DefaultImporter.ImportType(typeof(byte[]));
        var fieldInjectedArray = new FieldDefinition(name, FieldAttributes.Assembly | FieldAttributes.Static, new FieldSignature(byteArray.ToTypeSignature()));
        classWithLayout.Fields.Add(fieldInjectedArray);

        var systemByte = module.DefaultImporter.ImportType(module.CorLibTypeFactory.Byte.ToTypeDefOrRef());
        var initializeArrayMethod = module.DefaultImporter.ImportMethod(typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.InitializeArray), new Type[]
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
            new CilInstruction(CilOpCodes.Ldtoken, module.DefaultImporter.ImportField(fieldWithRVA)),
            new CilInstruction(CilOpCodes.Call, initializeArrayMethod),
            new CilInstruction(CilOpCodes.Stsfld, fieldInjectedArray),
        });
        return fieldInjectedArray;
    }
    public TypeDefinition CreateInvisibleType(ModuleDefinition module, string name = null)
    {
        var invislbeTypeDef = new TypeDefinition(null, name ?? "<PrivateImplementationDetails>", TypeAttributes.Public, module.CorLibTypeFactory.Object.ToTypeDefOrRef());
        InjectCompilerGeneratedAttribute(module, invislbeTypeDef);
        invislbeTypeDef.Attributes |= TypeAttributes.Sealed | TypeAttributes.ExplicitLayout;
        return invislbeTypeDef;
    }
    public TypeDefinition CreateInvisibleValueType(ModuleDefinition module, string name = null)
    {
        var invislbeValueType = new TypeDefinition(null, name ?? "<PrivateImplementationDetails>", TypeAttributes.NestedAssembly, module.DefaultImporter.ImportType(typeof(ValueType)));
        InjectCompilerGeneratedAttribute(module, invislbeValueType);
        invislbeValueType.IsAbstract = false;
        invislbeValueType.IsSealed = false;
        invislbeValueType.IsBeforeFieldInit = false;
        return invislbeValueType;
    }
    public TypeDefinition InjectInvisibleValueType(ModuleDefinition module, TypeDefinition type, string name = null)
    {
        var result = CreateInvisibleValueType(module, name);
        type.NestedTypes.Add(result);
        return result;
    }
    public CustomAttribute InjectCompilerGeneratedAttribute(ModuleDefinition module, TypeDefinition type = null)
    {
        var compilerGeneratedAttributeType = module.DefaultImporter.ImportType(typeof(CompilerGeneratedAttribute));
        var compilerGeneratedCtor = new MemberReference(compilerGeneratedAttributeType, ".ctor", MethodSignature.CreateInstance(module.CorLibTypeFactory.Void));
        var compilerGeneratedAttribute = new CustomAttribute(compilerGeneratedCtor);
        if (type != null)
        {
            type.CustomAttributes.Add(compilerGeneratedAttribute);
        }
        return compilerGeneratedAttribute;
    }
    public CustomAttribute InjectAttributeWithContent(ModuleDefinition module, string @namespace, string @name, string text)
    {
        var signature = MethodSignature.CreateInstance(module.CorLibTypeFactory.Void);
        var attributeCtor = module.CorLibTypeFactory.CorLibScope.CreateTypeReference(@namespace, @name)
            .CreateMemberReference(".ctor", signature);
        var customAttributeSignature = new CustomAttributeSignature(new CustomAttributeArgument(module.CorLibTypeFactory.String, text));
        var customAttribute = new CustomAttribute(attributeCtor, customAttributeSignature);
        module.CustomAttributes.Add(customAttribute);
        return customAttribute;
    }
    public CustomAttribute InjectAttribute(ModuleDefinition module, string @namespace, string @name)
    {
        var signature = MethodSignature.CreateInstance(module.CorLibTypeFactory.Void);
        var attributeCtor = module.CorLibTypeFactory.CorLibScope.CreateTypeReference(@namespace, @name)
            .CreateMemberReference(".ctor", signature);
        var customAttribute = new CustomAttribute(attributeCtor);
        module.CustomAttributes.Add(customAttribute);
        return customAttribute;
    }
}