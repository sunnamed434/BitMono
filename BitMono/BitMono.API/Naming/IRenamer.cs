using dnlib.DotNet;

namespace BitMono.API.Naming
{
    public interface IRenamer
    {
        string Rename();
        void Rename(IFullName fullName);
        void Rename(params IFullName[] fullNames);
        void Rename(IVariable variable);
    }
}