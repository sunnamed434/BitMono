namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime | MemberInclusionFlags.Model | MemberInclusionFlags.Reflection)]
public class FullRenamer : Protection
{
    private readonly Renamer _renamer;

    public FullRenamer(Renamer renamer, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _renamer = renamer;
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public override Task ExecuteAsync()
    {
        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method.DeclaringType?.IsModuleType == false && method is { IsConstructor: false, IsVirtual: false })
            {
                _renamer.Rename(method);
                if (method.HasParameters())
                {
                    foreach (var parameter in method.Parameters)
                    {
                        if (parameter.Definition != null)
                        {
                            _renamer.Rename(parameter.Definition);
                        }
                    }
                }
            }
        }
        foreach (var type in Context.Parameters.Members.OfType<TypeDefinition>())
        {
            if (type.IsModuleType == false)
            {
                _renamer.Rename(type);
            }
        }
        foreach (var field in Context.Parameters.Members.OfType<FieldDefinition>())
        {
            if (field.DeclaringType?.IsModuleType == false)
            {
                _renamer.Rename(field);
            }
        }
        return Task.CompletedTask;
    }
}