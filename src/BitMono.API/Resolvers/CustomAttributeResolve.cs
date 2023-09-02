namespace BitMono.API.Resolvers;

public class CustomAttributeResolve
{
    [NullGuard.AllowNull]
    public Dictionary<string, object>? NamedValues { get; set; }
    [NullGuard.AllowNull]
    public List<object>? FixedValues { get; set; }
    public CustomAttribute? Attribute { get; set; }
}