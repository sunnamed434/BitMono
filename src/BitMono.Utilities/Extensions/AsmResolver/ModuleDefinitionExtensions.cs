namespace BitMono.Utilities.Extensions.AsmResolver;

public static class ModuleDefinitionExtensions
{
    [return: AllowNull]
    public static TMember ResolveOrThrow<TMember>(this ModuleDefinition source, Type type)
    {
        if (source.TryLookupMember(new MetadataToken((uint)type.MetadataToken), out IMetadataMember metadataMember))
        {
            if (metadataMember is TMember member)
            {
                return member;
            }
        }
        throw new ArgumentException($"Unable to resolve member {type.FullName}");
    }
    public static IEnumerable<IMetadataMember> FindDefinitions(this ModuleDefinition source)
    {
        yield return source.Assembly;
        yield return source;
        foreach (var type in source.GetAllTypes())     
        {
            yield return type;
            foreach (var method in type.Methods)
            {
                yield return method;
            }
            foreach (var field in type.Fields)
            {
                yield return field;
            }
            foreach (var property in type.Properties)
            {
                yield return property;
            }
            foreach (var @event in type.Events)
            {
                yield return @event;
            }
        }
    }
}