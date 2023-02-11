namespace BitMono.Core.TestCases.Reflection;

public class ReflectionMethods
{
    public static void VoidMethod(string text)
    {
    }
    public void UsesReflectionOnItSelf()
    {
        typeof(ReflectionMethods).GetMethod(nameof(UsesReflectionOnItSelf));
    }
    public void Uses3Reflection()
    {
        typeof(ReflectionMethods).GetMethod(nameof(Uses3Reflection));
        typeof(ReflectionMethods).GetMethod(nameof(UsesReflectionOnItSelf));
        typeof(ReflectionMethods).GetMethod(nameof(VoidMethod), BindingFlags.Public | BindingFlags.Static);
    }
}