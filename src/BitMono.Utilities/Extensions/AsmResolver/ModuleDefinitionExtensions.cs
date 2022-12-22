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
    public static IEnumerable<IMemberDefinition> FindDefinitions(this ModuleDefinition source)
    {
        foreach (var typeDef in source.GetAllTypes())     
        {
            yield return typeDef;
            foreach (var methodDef in typeDef.Methods)
            {
                yield return methodDef;
            }
            foreach (var fieldDef in typeDef.Fields)
            {
                yield return fieldDef;
            }
            foreach (var propertyDef in typeDef.Properties)
            {
                yield return propertyDef;
            }
            foreach (var eventDef in typeDef.Events)
            {
                yield return eventDef;
            }
        }
    }
}