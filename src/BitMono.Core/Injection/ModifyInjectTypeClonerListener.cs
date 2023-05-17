namespace BitMono.Core.Injection;

[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
public class ModifyInjectTypeClonerListener : InjectTypeClonerListener
{
    public ModifyInjectTypeClonerListener(ModifyFlags modify, Renamer renamer, ModuleDefinition targetModule) : base(targetModule)
    {
        Modify = modify;
        Renamer = renamer;
    }

    public ModifyFlags Modify { get; }
    public Renamer Renamer { get; }

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
                var parameterDefinitions = method.ParameterDefinitions;
                for (var i = 0; i < parameterDefinitions.Count; i++)
                {
                    parameterDefinitions[i].Name = string.Empty;
                }
            }
        }
        base.OnClonedMember(original, cloned);
    }
}