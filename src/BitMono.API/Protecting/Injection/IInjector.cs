namespace BitMono.API.Protecting.Injection;

public interface IInjector
{
    FieldDefinition InjectInvisibleArray(ModuleDefinition module, TypeDefinition type, byte[] data, string name);
    TypeDefinition CreateInvisibleType(ModuleDefinition module, string name = null);
    TypeDefinition CreateInvisibleValueType(ModuleDefinition module, string name = null);
    TypeDefinition InjectInvisibleValueType(ModuleDefinition module, TypeDefinition type, string name = null);
    CustomAttribute InjectCompilerGeneratedAttribute(ModuleDefinition module, TypeDefinition type = null);
    CustomAttribute InjectAttributeWithContent(ModuleDefinition module, string @namespace, string @name, string text);
    CustomAttribute InjectAttribute(ModuleDefinition module, string @namespace, string @name);
}