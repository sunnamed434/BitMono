namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime | MemberInclusionFlags.Model | MemberInclusionFlags.Reflection)]
public class FullRenamer : Protection
{
    private readonly Renamer _renamer;

    public FullRenamer(Renamer renamer, ProtectionContext context) : base(context)
    {
        _renamer = renamer;
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        foreach (var method in parameters.Members.OfType<MethodDefinition>())
        {
            if (method.DeclaringType?.IsModuleType == false && method is { IsConstructor: false, IsVirtual: false })
            {
                _renamer.Rename(method);
                if (method.HasParameters())
                {
                    foreach (var parameter in method.Parameters)
                    {
                        _renamer.Rename(parameter.Definition);
                    }
                }
            }
        }
        foreach (var type in parameters.Members.OfType<TypeDefinition>())
        {
            if (type.IsModuleType == false)
            {
                _renamer.Rename(type);
            }
        }
        foreach (var field in parameters.Members.OfType<FieldDefinition>())
        {
            if (field.DeclaringType?.IsModuleType == false)
            {
                _renamer.Rename(field);
            }
        }
        return Task.CompletedTask;
    }
}