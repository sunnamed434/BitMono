namespace BitMono.Core.Protecting.Pipeline;

public class InvokablePipeline : IInvokablePipeline
{
    public InvokablePipeline(ProtectionContext context)
    {
        Context = context;
    }

    public bool Succeed { get; private set; } = true;
    public ProtectionContext Context { get; }
    [AllowNull]
    public Action OnFail { get; set; }

    public async Task InvokeAsync(Func<ProtectionContext, Task<bool>> func)
    {
        if (Succeed == false)
        {
            return;
        }
        Succeed = await func.Invoke(Context);
        if (Succeed == false)
        {
            OnFail?.Invoke();
        }
    }
}