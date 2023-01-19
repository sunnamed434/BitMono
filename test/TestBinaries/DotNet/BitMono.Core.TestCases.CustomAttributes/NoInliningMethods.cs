namespace BitMono.Core.TestCases.CustomAttributes;

public class NoInliningMethods
{
    public void VoidMethod()
    {
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void NoInliningMethod()
    {
    }
}