namespace BitMono.Benchmarks;

[MemoryDiagnoser]
public class RuntimeFrameworkInformationBenchmark
{
    [Benchmark]
    public void RuntimeInformation()
    {
        RuntimeUtilities.GetFrameworkInformation();
    }
}