namespace BitMono.Utilities.Extensions.AsmResolver;

public static class CloningExtensions
{
    public static MemberCloneResult RenameClonedMembers(this MemberCloneResult source, IRenamer renamer)
    {
        source.ClonedMembers.ForEach(member => renamer.Rename(member));
        return source;
    }
    public static MemberCloneResult RemoveNamespaceOfClonedMembers(this MemberCloneResult source, IRenamer renamer)
    {
        source.ClonedMembers.ForEach(member => renamer.RemoveNamespace(member));
        return source;
    }
}