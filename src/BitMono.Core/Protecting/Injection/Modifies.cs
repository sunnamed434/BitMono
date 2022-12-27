namespace BitMono.Core.Protecting.Injection;

[Flags]
public enum Modifies
{
    None = 0,
    Rename = 0x1,
    RemoveNamespace = 0x2,
    RenameAndRemoveNamespace = Rename | RemoveNamespace,
}