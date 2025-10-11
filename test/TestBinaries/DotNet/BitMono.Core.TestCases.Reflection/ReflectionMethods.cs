namespace BitMono.Core.TestCases.Reflection;

public class ReflectionMethods
{
    public string TestField = "test";
    private int _privateField = 42;
    public string TestProperty { get; set; } = "test";
    public int ReadOnlyProperty { get; } = 123;
    public event EventHandler TestEvent;

    public static void VoidMethod(string text)
    {
    }

    public void UsesReflectionOnItSelf()
    {
        _ = typeof(ReflectionMethods).GetMethod(nameof(UsesReflectionOnItSelf));
    }

    public void Uses3Reflection()
    {
        _ = typeof(ReflectionMethods).GetMethod(nameof(Uses3Reflection));
        _ = typeof(ReflectionMethods).GetMethod(nameof(UsesReflectionOnItSelf));
        _ = typeof(ReflectionMethods).GetMethod(nameof(VoidMethod), BindingFlags.Public | BindingFlags.Static);
    }

    public void UsesFieldReflection()
    {
        _ = typeof(ReflectionMethods).GetField(nameof(TestField));
    }

    public void UsesPrivateFieldReflection()
    {
        _ = typeof(ReflectionMethods).GetField("_privateField", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public void UsesPropertyReflection()
    {
        _ = typeof(ReflectionMethods).GetProperty(nameof(TestProperty));
    }

    public void UsesReadOnlyPropertyReflection()
    {
        _ = typeof(ReflectionMethods).GetProperty(nameof(ReadOnlyProperty));
    }

    public void UsesEventReflection()
    {
        _ = typeof(ReflectionMethods).GetEvent(nameof(TestEvent));
    }

    public void UsesGetMemberForMethod()
    {
        _ = typeof(ReflectionMethods).GetMember(nameof(UsesFieldReflection));
    }

    public void UsesGetMemberForField()
    {
        _ = typeof(ReflectionMethods).GetMember(nameof(TestField));
    }

    public void UsesGetMemberForProperty()
    {
        _ = typeof(ReflectionMethods).GetMember(nameof(TestProperty));
    }

    public void UsesGetMemberForEvent()
    {
        _ = typeof(ReflectionMethods).GetMember(nameof(TestEvent));
    }

    public void UsesVariableForMethodReflection()
    {
        string methodName = nameof(UsesFieldReflection);
        _ = typeof(ReflectionMethods).GetMethod(methodName);
    }

    public void UsesVariableForFieldReflection()
    {
        string fieldName = nameof(TestField);
        _ = typeof(ReflectionMethods).GetField(fieldName);
    }

    public void UsesVariableForPropertyReflection()
    {
        string propertyName = nameof(TestProperty);
        _ = typeof(ReflectionMethods).GetProperty(propertyName);
    }

    public void UsesVariableForEventReflection()
    {
        string eventName = nameof(TestEvent);
        _ = typeof(ReflectionMethods).GetEvent(eventName);
    }

    public void UsesTypeGetTypeFromHandle()
    {
        var typeHandle = typeof(ReflectionMethods).TypeHandle;
        _ = Type.GetTypeFromHandle(typeHandle);
    }

    public void UsesLdtokenForType()
    {
        _ = typeof(ReflectionMethods);
    }

    public void UsesMultipleReflectionTypes()
    {
        _ = typeof(ReflectionMethods).GetMethod(nameof(UsesFieldReflection));
        _ = typeof(ReflectionMethods).GetField(nameof(TestField));
        _ = typeof(ReflectionMethods).GetProperty(nameof(TestProperty));
        _ = typeof(ReflectionMethods).GetEvent(nameof(TestEvent));
    }

    public void UsesReflectionWithBindingFlags()
    {
        _ = typeof(ReflectionMethods).GetMethod(nameof(UsesFieldReflection), BindingFlags.Public | BindingFlags.Instance);
        _ = typeof(ReflectionMethods).GetField(nameof(TestField), BindingFlags.Public | BindingFlags.Instance);
        _ = typeof(ReflectionMethods).GetProperty(nameof(TestProperty), BindingFlags.Public | BindingFlags.Instance);
        _ = typeof(ReflectionMethods).GetEvent(nameof(TestEvent), BindingFlags.Public | BindingFlags.Instance);
    }

    public void UsesAllReflectionTypes()
    {
        _ = typeof(ReflectionMethods).GetMethod(nameof(UsesFieldReflection));
        _ = typeof(ReflectionMethods).GetField(nameof(TestField));
        _ = typeof(ReflectionMethods).GetProperty(nameof(TestProperty));
        _ = typeof(ReflectionMethods).GetEvent(nameof(TestEvent));
        _ = typeof(ReflectionMethods).GetMember(nameof(UsesFieldReflection));
        var typeHandle = typeof(ReflectionMethods).TypeHandle;
        _ = Type.GetTypeFromHandle(typeHandle);
    }

    public void UsesComplexReflectionPatterns()
    {
        var type = typeof(ReflectionMethods);

        var methodName = nameof(UsesFieldReflection);
        var fieldName = nameof(TestField);
        var propertyName = nameof(TestProperty);
        var eventName = nameof(TestEvent);

        _ = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        _ = type.GetMethod(methodName, new[] { typeof(string) });
        _ = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);

        _ = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        _ = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        _ = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);

        _ = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        _ = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
        _ = type.GetProperty(propertyName, new[] { typeof(string) });

        _ = type.GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
        _ = type.GetEvent(eventName, BindingFlags.NonPublic | BindingFlags.Instance);

        _ = type.GetMember(methodName, BindingFlags.Public | BindingFlags.Instance);
        _ = type.GetMember(fieldName, BindingFlags.Public | BindingFlags.Instance);
        _ = type.GetMember(propertyName, BindingFlags.Public | BindingFlags.Instance);
        _ = type.GetMember(eventName, BindingFlags.Public | BindingFlags.Instance);

        _ = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
        _ = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        _ = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        _ = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        _ = type.GetEvents(BindingFlags.Public | BindingFlags.Instance);

        var typeHandle = type.TypeHandle;
        _ = Type.GetTypeFromHandle(typeHandle);

        var assembly = type.Assembly;
        _ = assembly.GetType("BitMono.Core.TestCases.Reflection.ReflectionMethods");

        var baseType = type.BaseType;
        if (baseType != null)
        {
            _ = baseType.GetMethod("ToString");
            _ = baseType.GetProperty("Name");
        }

        var interfaces = type.GetInterfaces();
        foreach (var iface in interfaces)
        {
            _ = iface.GetMethod("GetHashCode");
        }

        var genericType = typeof(List<>);
        _ = genericType.GetMethod("Add");
        _ = genericType.GetProperty("Count");

        var constructedType = typeof(List<string>);
        _ = constructedType.GetMethod("Add");
        _ = constructedType.GetProperty("Count");

        var nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var nestedType in nestedTypes)
        {
            _ = nestedType.GetMethod("ToString");
        }
    }
}