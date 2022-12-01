using dnlib.DotNet.Emit;

namespace BitMono.Utilities.Extensions.dnlib
{
    public static class CILBodyExtensions
    {
        public static CilBody SimplifyAndOptimizeBranches(this CilBody source)
        {
            source.SimplifyBranches();
            source.OptimizeBranches();
            return source;
        }
    }
}