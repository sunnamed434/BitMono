namespace BitMono.Utilities.Extensions.AsmResolver;

public static class ModuleDefinitionExtensions
{
    [return: AllowNull]
    public static TMember ResolveOrThrow<TMember>(this ModuleDefinition source, Type type)
    {
        if (source.TryLookupMember(new MetadataToken((uint)type.MetadataToken), out var metadataMember))
        {
            if (metadataMember is TMember member)
            {
                return member;
            }
        }
        throw new ArgumentException($"Unable to resolve member {type.FullName}");
    }
    [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
    [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
    public static List<IMetadataMember> FindMembers(this ModuleDefinition source)
    {
        var members = new List<IMetadataMember>();
        members.Add(source);
        members.Add(source.Assembly);
        foreach (var type in source.GetAllTypes())
        {
            members.Add(type);
            members.AddRange(type.Methods);
            members.AddRange(type.Fields);
            members.AddRange(type.Properties);
            members.AddRange(type.Events);
        }
        return members;
    }
}