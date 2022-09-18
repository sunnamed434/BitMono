using dnlib.DotNet;
using System.Threading.Tasks;

namespace BitMono.Core.Extensions
{
    public static class TypeSigExtensions
    {
        public static bool IsTask(this TypeSig source)
        {
            return source.FullName.Contains(nameof(Task));
        }
    }
}