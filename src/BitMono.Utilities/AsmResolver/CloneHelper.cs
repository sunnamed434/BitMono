namespace BitMono.Utilities.AsmResolver;

public static class CloneHelper
{
    /// <summary>
    /// Clones via AsmResolver's API,
    /// but removes the <see cref="cloningModule"/> assembly reference from <see cref="sourceModule"/>.
    /// This is needed as a workaround, because sometimes AsmResolver adds
    /// a reference of the included item to a <see cref="sourceModule"/>.
    /// See here more details about that problem: https://github.com/sunnamed434/BitMono/issues/207
    /// </summary>
    /// <param name="cloner">The cloner.</param>
    /// <param name="sourceModule">The source module.</param>
    /// <param name="cloningModule">The cloning module.</param>
    /// <returns>The clone result.</returns>
    public static MemberCloneResult CloneSafely(this MemberCloner cloner,
        ModuleDefinition sourceModule, ModuleDefinition cloningModule)
    {
        var cloningModuleAssembly = cloningModule.Assembly
                                    ?? throw new InvalidOperationException($"{nameof(cloningModule)} assembly was null");

        var result = cloner.Clone();

        var fullName = cloningModuleAssembly.FullName;
        if (sourceModule.AssemblyReferences.FirstOrDefault(
                x => x.FullName == fullName) is { } assembly)
        {
            sourceModule.AssemblyReferences.Remove(assembly);
        }

        return result;
    }
}