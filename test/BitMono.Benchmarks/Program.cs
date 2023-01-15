using System.Reflection;
using BenchmarkDotNet.Running;

namespace BitMono.Benchmarks;

internal static class Program
{
    private static void Main(string[] args)
    {
        BenchmarkRunner.Run(Assembly.GetExecutingAssembly());
    }
}