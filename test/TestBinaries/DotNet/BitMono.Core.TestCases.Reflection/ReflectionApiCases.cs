namespace BitMono.Core.TestCases.Reflection;

public enum Color
{
    Red,
    Green,
    Blue
}

// Same member name on two unrelated types; only ProbeReflected.Shared is reflected (in
// ReflectionApiCases.ReflectsOnOthers). Proves identity-based exclusion: ProbeUntouched.Shared must
// stay obfuscatable even though it shares the name - the old name-only matcher froze both.
public class ProbeReflected
{
    public void Shared()
    {
    }
}

public class ProbeUntouched
{
    public void Shared()
    {
    }
}

// Referenced only via a bare typeof - must NOT be excluded (no name lookup flows from it).
public class BareTypeofProbe
{
}

// Resolved by Type.GetType(string) - must be excluded.
public class GetTypeProbe
{
}

public class BaseProbe
{
    public virtual void Virt()
    {
    }
}

public class DerivedProbe : BaseProbe
{
    public override void Virt()
    {
    }
}

public class ReflectionApiCases
{
    public string ApiField = "x";
    public string ApiProperty { get; set; } = "x";

    public static void Target()
    {
    }

    public static void DelegateTarget()
    {
    }

    public void Untouched()
    {
    }

    public void IlTarget()
    {
    }

    public class Nested
    {
    }

    // Reflects only on OTHER members, never on itself - the caller must stay renameable.
    public void ReflectsOnOthers()
    {
        _ = typeof(ProbeReflected).GetMethod(nameof(ProbeReflected.Shared));
        _ = typeof(ReflectionApiCases).GetField(nameof(ApiField));
    }

    public void UsesGetRuntimeMethod()
    {
        _ = typeof(ReflectionApiCases).GetRuntimeMethod(nameof(Target), Type.EmptyTypes);
    }

    public void UsesGetRuntimeProperty()
    {
        _ = typeof(ReflectionApiCases).GetRuntimeProperty(nameof(ApiProperty));
    }

    public void UsesCreateDelegate()
    {
        _ = Delegate.CreateDelegate(typeof(Action), typeof(ReflectionApiCases), nameof(DelegateTarget));
    }

    public void UsesEnumParse()
    {
        _ = Enum.Parse(typeof(Color), nameof(Color.Red));
    }

    public void UsesGetTypeByName()
    {
        _ = Type.GetType("BitMono.Core.TestCases.Reflection.GetTypeProbe");
    }

    public void UsesBareTypeof()
    {
        _ = typeof(BareTypeofProbe);
    }

    public void UsesGetNestedType()
    {
        _ = typeof(ReflectionApiCases).GetNestedType(nameof(Nested));
    }

    public void ReflectsOverride()
    {
        _ = typeof(DerivedProbe).GetMethod(nameof(DerivedProbe.Virt));
    }

    public void ReadsIl()
    {
        var method = typeof(ReflectionApiCases).GetMethod(nameof(IlTarget));
        _ = method.GetMethodBody().GetILAsByteArray();
    }
}
