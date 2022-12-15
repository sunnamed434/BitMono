namespace BitMono.Utilities.Extensions.dnlib;

public static class ParameterExtensions
{
    public static bool IsNullable(this Parameter source)
    {
        return source.Type.TypeName.StartsWith(nameof(Nullable));
    }
}