namespace BitMono.Core.Protecting.Injection;

[Flags]
public enum Modifies
{
    Rename = 0x1,
    RemoveNamespace = 0x2,
    EmptyMethodParameterName = 0x4,
    All = Rename | RemoveNamespace | EmptyMethodParameterName
}