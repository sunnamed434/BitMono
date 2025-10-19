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

    public void DeepReflectionLevel1()
    {
        DeepReflectionLevel2();
    }

    public void DeepReflectionLevel2()
    {
        DeepReflectionLevel3();
    }

    public void DeepReflectionLevel3()
    {
        UsesReflectionOnItSelf();
    }

    public void DeepReflectionDirect()
    {
        typeof(ReflectionMethods).GetMethod(nameof(DeepReflectionDirect));
    }

    public void NonReflectionMethod()
    {
        VoidMethod("test");
    }

    public void CallsNonReflectionMethod()
    {
        NonReflectionMethod();
    }

    // Base type reflection test cases
    public class BaseClass
    {
        public virtual void BaseMethod() { }
        public string BaseField = "base";
        public string BaseProperty { get; set; } = "base";
        public event EventHandler BaseEvent;
    }

    public class DerivedClass : BaseClass
    {
        public override void BaseMethod() { }
        public void DerivedMethod() { }
    }

    public void UsesBaseTypeReflection()
    {
        _ = typeof(DerivedClass).GetMethod("BaseMethod");
        _ = typeof(DerivedClass).GetField("BaseField");
        _ = typeof(DerivedClass).GetProperty("BaseProperty");
        _ = typeof(DerivedClass).GetEvent("BaseEvent");
    }

    public void UsesInheritedMemberReflection()
    {
        _ = typeof(DerivedClass).GetMethod("DerivedMethod");
        _ = typeof(DerivedClass).GetMember("BaseMethod");
    }

    // Plural reflection methods test cases
    public void UsesGetMethods()
    {
        _ = typeof(ReflectionMethods).GetMethods();
        _ = typeof(ReflectionMethods).GetMethods(BindingFlags.Public | BindingFlags.Instance);
    }

    public void UsesGetFields()
    {
        _ = typeof(ReflectionMethods).GetFields();
        _ = typeof(ReflectionMethods).GetFields(BindingFlags.Public | BindingFlags.Instance);
    }

    public void UsesGetProperties()
    {
        _ = typeof(ReflectionMethods).GetProperties();
        _ = typeof(ReflectionMethods).GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

    public void UsesGetEvents()
    {
        _ = typeof(ReflectionMethods).GetEvents();
        _ = typeof(ReflectionMethods).GetEvents(BindingFlags.Public | BindingFlags.Instance);
    }

    public void UsesGetMembers()
    {
        _ = typeof(ReflectionMethods).GetMembers();
        _ = typeof(ReflectionMethods).GetMembers(BindingFlags.Public | BindingFlags.Instance);
    }

    // Generic type reflection test cases
    public void UsesGenericTypeReflection()
    {
        _ = typeof(List<>).GetMethod("Add");
        _ = typeof(List<>).GetProperty("Count");
        _ = typeof(Dictionary<,>).GetMethod("Add");
    }

    public void UsesConstructedGenericTypeReflection()
    {
        _ = typeof(List<string>).GetMethod("Add");
        _ = typeof(List<string>).GetProperty("Count");
        _ = typeof(Dictionary<string, int>).GetMethod("Add");
    }

    // Assembly.GetType test cases
    public void UsesAssemblyGetType()
    {
        var assembly = typeof(ReflectionMethods).Assembly;
        var type = assembly.GetType("BitMono.Core.TestCases.Reflection.ReflectionMethods");
        _ = type?.GetMethod("UsesAssemblyGetType");
    }

    public void UsesAssemblyGetTypeWithReflection()
    {
        var assembly = typeof(ReflectionMethods).Assembly;
        var type = assembly.GetType("BitMono.Core.TestCases.Reflection.ReflectionMethods");
        _ = type?.GetMethod("UsesAssemblyGetTypeWithReflection");
        _ = type?.GetField("TestField");
        _ = type?.GetProperty("TestProperty");
    }

    // Complex reflection patterns
    public void UsesComplexTypeResolution()
    {
        var typeName = "BitMono.Core.TestCases.Reflection.ReflectionMethods";
        var assembly = typeof(ReflectionMethods).Assembly;
        var type = assembly.GetType(typeName);
        
        if (type != null)
        {
            _ = type.GetMethod("UsesComplexTypeResolution");
            _ = type.GetField("TestField");
            _ = type.GetProperty("TestProperty");
            _ = type.GetEvent("TestEvent");
        }
    }

    public void UsesNestedTypeReflection()
    {
        _ = typeof(ReflectionMethods).GetNestedTypes();
        _ = typeof(ReflectionMethods).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
    }

    public void UsesInterfaceReflection()
    {
        var interfaces = typeof(ReflectionMethods).GetInterfaces();
        foreach (var iface in interfaces)
        {
            _ = iface.GetMethod("GetHashCode");
            _ = iface.GetMethod("ToString");
        }
    }

    public void UsesMemberOverrideReflection()
    {
        _ = typeof(DerivedClass).GetMethod("BaseMethod");
        _ = typeof(DerivedClass).GetMethod("VirtualMethod");
        _ = typeof(DerivedClass).GetProperty("BaseProperty");
        _ = typeof(DerivedClass).GetProperty("VirtualProperty");
        _ = typeof(DerivedClass).GetEvent("BaseEvent");
        _ = typeof(DerivedClass).GetEvent("VirtualEvent");
    }
}

public class BaseClass
{
    public virtual void BaseMethod() { }
    public virtual void VirtualMethod() { }
    public virtual string BaseProperty { get; set; }
    public virtual string VirtualProperty { get; set; }
    public virtual event EventHandler BaseEvent;
    public virtual event EventHandler VirtualEvent;
}

public class DerivedClass : BaseClass
{
    public override void BaseMethod() { }
    public override void VirtualMethod() { }
    public override string BaseProperty { get; set; }
    public override string VirtualProperty { get; set; }
    public override event EventHandler BaseEvent;
    public override event EventHandler VirtualEvent;
}