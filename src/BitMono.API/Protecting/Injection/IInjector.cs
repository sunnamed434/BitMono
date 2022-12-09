using dnlib.DotNet;

namespace BitMono.API.Protecting.Injection
{
    public interface IInjector
    {
        FieldDef InjectInvisibleArray(ModuleDefMD moduleDefMD, TypeDef typeDef, byte[] data, string name);
        TypeDef CreateInvisibleType(ModuleDefMD moduleDefMD, string name = null);
        TypeDef CreateInvisibleValueType(ModuleDefMD moduleDefMD, string name = null);
        TypeDef InjectInvisibleValueType(ModuleDefMD moduleDefMD, TypeDef typeDef, string name = null);
        CustomAttribute InjectCompilerGeneratedAttribute(ModuleDefMD moduleDefMD, TypeDef typeDef = null);
        void InjectAttributeWithContent(ModuleDefMD moduleDefMD, string @namespace, string @name, string text);
        void InjectAttribute(ModuleDefMD moduleDefMD, string @namespace, string @name);
    }
}