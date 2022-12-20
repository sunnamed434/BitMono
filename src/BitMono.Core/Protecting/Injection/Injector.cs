namespace BitMono.Core.Protecting.Injection;

public class Injector : IInjector
{
    public FieldDefinition InjectInvisibleArray(ModuleDefinition module, TypeDefinition type, byte[] data, string name)
    {
        var valueTypeRef = module.DefaultImporter.ImportType(typeof(ValueType));
        var classWithLayout = new TypeDefinition(null, "<>c", TypeAttributes.Sealed | TypeAttributes.ExplicitLayout, valueTypeRef);
        classWithLayout.ClassLayout = new AsmResolver.DotNet.ClassLayout(1, (uint)data.Length);
        var compilerGeneratedAttribute = InjectCompilerGeneratedAttribute(module);
        classWithLayout.CustomAttributes.Add(compilerGeneratedAttribute);

        type.NestedTypes.Add(classWithLayout);

        var fieldWithRVA = new FieldDefinition("dummy", FieldAttributes.Static | FieldAttributes.Assembly | FieldAttributes.HasFieldRva, new FieldSignature(classWithLayout.ToTypeSignature()));
        //fieldWithRVA.InitialValue = data;
        classWithLayout.Fields.Add(fieldWithRVA);

        var byteArrayRef = module.DefaultImporter.ImportType(typeof(byte[]));
        var fieldInjectedArray = new FieldDefinition(name, FieldAttributes.Static | FieldAttributes.Assembly, new FieldSignature(byteArrayRef.ToTypeSignature()));
        classWithLayout.Fields.Add(fieldInjectedArray);

        var systemByte = module.DefaultImporter.ImportType(typeof(byte));
        var initializeArrayMethod = module.DefaultImporter.ImportMethod(typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.InitializeArray), new Type[]
        {
            typeof(Array),
            typeof(RuntimeFieldHandle)
        }));

        var cctor = classWithLayout.GetOrCreateStaticConstructor();
        var cctorBodyInstructions = cctor.CilMethodBody.Instructions;
        cctorBodyInstructions.Add(new CilInstruction(CilOpCodes.Ldc_I4, data.Length));
        cctorBodyInstructions.Add(new CilInstruction(CilOpCodes.Newarr, systemByte));
        cctorBodyInstructions.Add(new CilInstruction(CilOpCodes.Dup));
        cctorBodyInstructions.Add(new CilInstruction(CilOpCodes.Ldtoken, fieldWithRVA));
        cctorBodyInstructions.Add(new CilInstruction(CilOpCodes.Call, initializeArrayMethod));
        cctorBodyInstructions.Add(new CilInstruction(CilOpCodes.Stsfld, fieldInjectedArray));
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
        var invislbeTypeDef = new TypeDefinition(null, name ?? "<PrivateImplementationDetails>", TypeAttributes.NestedAssembly, module.DefaultImporter.ImportType(typeof(ValueType)));
        InjectCompilerGeneratedAttribute(module, invislbeTypeDef);
        invislbeTypeDef.IsAbstract = false;
        invislbeTypeDef.IsSealed = false;
        invislbeTypeDef.IsBeforeFieldInit = false;
        return invislbeTypeDef;
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
        var attributeTypeReference = module.CorLibTypeFactory.CorLibScope.CreateTypeReference(@namespace, @name);
        var attributeCtor = new MemberReference(attributeTypeReference, null, MethodSignature.CreateInstance(module.CorLibTypeFactory.Void));
        var customAttribute = new CustomAttribute(attributeCtor);
        customAttribute.Signature.FixedArguments.Add(new CustomAttributeArgument(module.CorLibTypeFactory.String, text));
        module.CustomAttributes.Add(customAttribute);
        return customAttribute;
    }
    public CustomAttribute InjectAttribute(ModuleDefinition module, string @namespace, string @name)
    {
        var attributeTypeReference = module.CorLibTypeFactory.CorLibScope.CreateTypeReference(@namespace, @name);
        var attributeCtor = new MemberReference(attributeTypeReference, null, MethodSignature.CreateInstance(module.CorLibTypeFactory.Void));
        var customAttribute = new CustomAttribute(attributeCtor);
        module.CustomAttributes.Add(customAttribute);
        return customAttribute;
    }
}