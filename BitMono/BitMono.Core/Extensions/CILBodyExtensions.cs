using dnlib.DotNet.Emit;

namespace BitMono.Core.Extensions
{
    public static class CILBodyExtensions
    {
        public static void SimplifyAndOptimizeBranches(this CilBody source)
        {
            source.SimplifyBranches();
            source.OptimizeBranches();
        }
    }
}