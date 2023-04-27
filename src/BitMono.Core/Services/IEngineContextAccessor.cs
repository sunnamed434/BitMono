namespace BitMono.Core.Services;

public interface IEngineContextAccessor
{
    StarterContext Instance { get; set; }
}