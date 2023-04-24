namespace BitMono.Core.Services;

public interface IEngineContextAccessor
{
    EngineContext Instance { get; set; }
}