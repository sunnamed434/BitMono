namespace BitMono.API.Pipeline;

public interface IInvokablePipeline
{
    bool Succeed { get; }
    Action? OnFail { get; set; }

    Task InvokeAsync(Func<IInvokablePipeline, Task<bool>> func);
    Task InvokeAsync(Func<Task<bool>> func);
}