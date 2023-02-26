namespace BitMono.Core.Injection;

[Flags]
public enum ModifyFlags
{
    Rename = 0x1,
    RemoveNamespace = 0x2,
    EmptyMethodParameterName = 0x4,
    All = Rename | RemoveNamespace | EmptyMethodParameterName
}