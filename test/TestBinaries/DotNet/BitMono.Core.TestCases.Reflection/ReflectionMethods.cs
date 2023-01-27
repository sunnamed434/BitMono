namespace BitMono.Core.TestCases.Reflection;

public class ReflectionMethods
{
    public void UsesReflectionOnItSelf()
    {
        typeof(ReflectionMethods).GetMethod(nameof(UsesReflectionOnItSelf));
    }
}