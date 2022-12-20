namespace BitMono.API.Protecting.Resolvers;

public interface IMemberDefinitionfResolver
{
    bool Resolve(string feature, IMemberDefinition memberDefenition);
}