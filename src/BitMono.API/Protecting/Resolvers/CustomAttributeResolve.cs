namespace BitMono.API.Protecting.Resolvers;

public class CustomAttributeResolve
{
    [AllowNull]
    public Dictionary<string, object> NamedValues { get; set; }
    [AllowNull]
    public List<object> FixedValues { get; set; }
    public CustomAttribute Attribute { get; set; }
}