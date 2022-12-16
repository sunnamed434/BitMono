using dnlib.DotNet;

namespace BitMono.Utilities.Extensions.dnlib;

public static class FieldDefExtensions
{
    public static FieldDef SetDeclaringTypeToNull(this FieldDef source)
    {
        source.DeclaringType = null;
        return source;
    }
}