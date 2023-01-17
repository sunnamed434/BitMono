namespace BitMono.Core.Protecting.Injection;

public class CustomInjector
{
    public CustomAttribute InjectAttribute(ModuleDefinition module, string @namespace, string name)
    {
        var factory = module.CorLibTypeFactory;
        var attributeReference = new TypeReference(module, module, @namespace, name);
        var signature = MethodSignature.CreateInstance(factory.Void);
        var attributeCtor = new MemberReference(attributeReference, ".ctor", signature);
        var attribute = new CustomAttribute(attributeCtor);
        module.CustomAttributes.Add(attribute);
        return attribute;
    }
    public CustomAttribute InjectAttribute(ModuleDefinition module, string @namespace, string name, string content)
    {
        var factory = module.CorLibTypeFactory;
        var attributeReference = new TypeReference(module, module, @namespace, name);
        var signature = MethodSignature.CreateInstance(factory.Void, factory.String);
        var attributeCtor = new MemberReference(attributeReference, ".ctor", signature);
        var customAttribute = new CustomAttribute(attributeCtor);
        customAttribute.Signature.FixedArguments.Add(new CustomAttributeArgument(factory.String, content));
        module.CustomAttributes.Add(customAttribute);
        return customAttribute;
    }
}