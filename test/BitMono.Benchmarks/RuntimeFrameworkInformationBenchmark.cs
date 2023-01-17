namespace BitMono.Benchmarks;

[SimpleJob(RuntimeMoniker.Net461)]
[SimpleJob(RuntimeMoniker.Net47)]
[MemoryDiagnoser]
public class RuntimeFrameworkInformationBenchmark
{
    [Benchmark]
    public void RuntimeInformation()
    {
        RuntimeUtilities.GetFrameworkInformation();
    }
}