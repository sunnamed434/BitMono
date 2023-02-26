namespace BitMono.Core.Pipeline;

public class InvokablePipeline : IInvokablePipeline
{
    public bool Succeed { get; private set; } = true;
    public Action? OnFail { get; set; } = null;

    public async Task InvokeAsync(Func<Task<bool>> func)
    {
        if (Succeed == false)
        {
            return;
        }
        Succeed = await func.Invoke();
        if (Succeed == false)
        {
            OnFail?.Invoke();
        }
    }
    public async Task InvokeAsync(Func<IInvokablePipeline, Task<bool>> func)
    {
        if (Succeed == false)
        {
            return;
        }
        Succeed = await func.Invoke(this);
        if (Succeed == false)
        {
            OnFail?.Invoke();
        }
    }
}