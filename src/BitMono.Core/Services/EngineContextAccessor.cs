namespace BitMono.Core.Services;

public class EngineContextAccessor : IEngineContextAccessor
{
    public StarterContext Instance { get; set; }
}