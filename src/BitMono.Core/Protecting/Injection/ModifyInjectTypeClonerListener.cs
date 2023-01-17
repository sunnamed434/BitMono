namespace BitMono.Core.Protecting.Injection;

public class ModifyInjectTypeClonerListener : InjectTypeClonerListener
{
    public ModifyInjectTypeClonerListener(ModifyFlags modify, IRenamer renamer, ModuleDefinition targetModule) : base(targetModule)
    {
        Modify = modify;
        Renamer = renamer;
    }

    public ModifyFlags Modify { get; }
    public IRenamer Renamer { get; }

    public override void OnClonedMember(IMemberDefinition original, IMemberDefinition cloned)
    {
        if (Modify.HasFlag(ModifyFlags.Rename))
        {
            Renamer.Rename(cloned);
        }
        if (Modify.HasFlag(ModifyFlags.RemoveNamespace))
        {
            Renamer.RemoveNamespace(cloned);
        }
        if (Modify.HasFlag(ModifyFlags.EmptyMethodParameterName))
        {
            if (cloned is MethodDefinition method)
            {
                foreach (var parameter in method.ParameterDefinitions)
                {
                    parameter.Name = string.Empty;
                }
            }
        }
        base.OnClonedMember(original, cloned);
    }
}