using BitMono.API.Protecting.Context;
using dnlib.DotNet;

namespace BitMono.API.Protecting.Renaming
{
    public interface IRenamer
    {
        string RenameUnsafely();
        void Rename(ProtectionContext context, IDnlibDef dnlibDef);
        void Rename(ProtectionContext context, TypeDef typeDef);
        void Rename(ProtectionContext context, MethodDef methodDef);
        void Rename(ProtectionContext context, FieldDef fieldDef);
        void Rename(ProtectionContext context, IFullName fullName);
        void Rename(ProtectionContext context, params IFullName[] fullNames);
        void Rename(ProtectionContext context, IVariable variable);
    }
}