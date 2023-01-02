namespace BitMono.API.Protecting.Pipeline;

public interface IInvokablePipeline
{
    bool Succeed { get; }
    ProtectionContext Context { get; }
    Action OnFail { get; set; }

    Task InvokeAsync(Func<ProtectionContext, Task<bool>> func);
}