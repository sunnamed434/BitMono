namespace BitMono.Core.Pipeline;

public class InvokablePipeline
{
    public bool Succeed { get; private set; } = true;
    public Func<Task>? OnFail { get; set; }

    public Task InvokeAsync(Action invokeMethod)
    {
        return InvokeInternalAsync(invokeMethod);
    }
    public Task InvokeAsync(Func<bool> invokeMethod)
    {
        return InvokeInternalAsync(invokeMethod);
    }
    public Task InvokeAsync(Func<Task<bool>> invokeMethod)
    {
        return InvokeInternalAsync(invokeMethod);
    }
    public Task InvokeAsync(Func<Task> invokeMethod)
    {
        return InvokeInternalAsync(invokeMethod);
    }

    private async Task InvokeInternalAsync(Delegate invokeMethod)
    {
        if (Succeed == false)
        {
            return;
        }

        Succeed = await InvokeMethodAsync(invokeMethod);
        if (Succeed == false && OnFail != null)
        {
            await OnFail();
        }
    }
    private static async Task<bool> InvokeMethodAsync(Delegate invokeMethod)
    {
        switch (invokeMethod)
        {
            case Action invoke:
                invoke();
                return true;
            case Func<Task<bool>> invoke:
                return await invoke();
            case Func<Task> invoke:
                await invoke();
                return true;
            case Func<bool> invoke:
                return invoke();
            default:
                throw new ArgumentOutOfRangeException(nameof(invokeMethod));
        }
    }
}