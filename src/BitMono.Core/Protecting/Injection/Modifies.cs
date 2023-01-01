namespace BitMono.Core.Protecting.Injection;

[Flags]
public enum Modifies
{
    Rename = 0x1,
    RemoveNamespace = 0x2,
    RenameAndRemoveNamespace = Rename | RemoveNamespace,
}