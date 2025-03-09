namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime | MemberInclusionFlags.Model | MemberInclusionFlags.Reflection)]
public class FullRenamer : Protection
{
    private readonly Renamer _renamer;

    public FullRenamer(Renamer renamer, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _renamer = renamer;
    }

    public override Task ExecuteAsync()
    {
        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method.DeclaringType?.IsModuleType == true)
            {
                continue;
            }
            if (method.IsConstructor || method.IsVirtual)
            {
                continue;
            }
            if (method.IsCompilerGenerated())
            {
                continue;
            }
            _renamer.Rename(method);
            if (!method.HasParameters())
            {
                continue;
            }
            foreach (var parameter in method.Parameters)
            {
                if (parameter.Definition == null)
                {
                    continue;
                }
                _renamer.Rename(parameter.Definition);
            }
        }
        foreach (var type in Context.Parameters.Members.OfType<TypeDefinition>())
        {
            if (type.IsModuleType)
            {
                continue;
            }
            if (type.IsCompilerGenerated())
            {
                continue;
            }
            _renamer.Rename(type);
        }
        foreach (var field in Context.Parameters.Members.OfType<FieldDefinition>())
        {
            if (field.DeclaringType?.IsModuleType == true)
            {
                continue;
            }
            if (field.IsCompilerGenerated())
            {
                continue;
            }
            _renamer.Rename(field);
        }
        return Task.CompletedTask;
    }
}