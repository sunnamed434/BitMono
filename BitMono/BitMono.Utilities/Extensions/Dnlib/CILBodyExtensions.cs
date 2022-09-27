using dnlib.DotNet.Emit;

namespace BitMono.Utilities.Extensions.Dnlib
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