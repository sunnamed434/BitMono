namespace BitMono.Core.Pipeline;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class InvokablePipeline
{
    public bool Succeed { get; private set; } = true;
    public Func<Task>? OnFail { get; set; }

    public async Task InvokeAsync(Func<Task<bool>> func)
    {
        if (Succeed == false)
        {
            return;
        }
        Succeed = await func.Invoke();
        if (Succeed == false)
        {
            if (OnFail != null)
            {
                await OnFail.Invoke();
            }
        }
    }
}