namespace BitMono.Utilities.AsmResolver;

public static class ModuleDefinitionExtensions
{
    [return: NullGuard.AllowNull]
    public static TMember ResolveOrThrow<TMember>(this ModuleDefinition source, Type type)
        where TMember : class, IMetadataMember
    {
        if (source.TryLookupMember(new MetadataToken((uint)type.MetadataToken), out TMember? member))
        {
            return member;
        }
        throw new ArgumentException($"Unable to resolve member {type.FullName}");
    }
    [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
    [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
    public static List<IMetadataMember> FindMembers(this ModuleDefinition source)
    {
        var members = new List<IMetadataMember>();
        members.Add(source);
        if (source.Assembly != null)
        {
            members.Add(source.Assembly);
        }
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