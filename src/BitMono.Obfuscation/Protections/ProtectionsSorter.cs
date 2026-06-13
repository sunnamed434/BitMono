namespace BitMono.Obfuscation.Protections;

public class ProtectionsSorter
{
    private readonly ObfuscationAttributeResolver _obfuscationAttributeResolver;
    private readonly AssemblyDefinition _assemblyDefinition;

    public ProtectionsSorter(ObfuscationAttributeResolver obfuscationAttributeResolver, AssemblyDefinition assemblyDefinition)
    {
        _obfuscationAttributeResolver = obfuscationAttributeResolver;
        _assemblyDefinition = assemblyDefinition;
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public ProtectionsSort Sort(List<IProtection> protections, IEnumerable<ProtectionSetting> protectionSettings, bool il2cpp = false)
    {
        var protectionsResolve = new ProtectionsResolver(protections, protectionSettings).Sort();
        var obfuscationAttributeProtections =
            protectionsResolve.FoundProtections.Where(x =>
                _obfuscationAttributeResolver.Resolve(x.GetName(), _assemblyDefinition));
        var deprecatedProtections =
            protectionsResolve.FoundProtections.Where(x => x.TryGetObsoleteAttribute(out _));
        // Skip protections that break il2cpp.exe's C++ conversion or only touch the discarded managed PE. See #250.
        var il2cppIncompatibleProtections = il2cpp
            ? protectionsResolve.FoundProtections.Where(x => x.IsIL2CPPIncompatible()).ToList()
            : new List<IProtection>();
        var sortedProtections = protectionsResolve.FoundProtections
            .Except(obfuscationAttributeProtections)
            .Except(deprecatedProtections)
            .Except(il2cppIncompatibleProtections);
        var pipelineProtections = sortedProtections
            .Where(x => x is IPipelineProtection)
            .Cast<IPipelineProtection>();
        var packers = sortedProtections
            .Where(x => x is IPacker)
            .Cast<IPacker>();
        sortedProtections = sortedProtections
            .Except(packers)
            .Except(pipelineProtections);
        var allProtections = sortedProtections.Concat(pipelineProtections).Concat(packers);

        var configureForNativeCodeProtections = allProtections.Where(
            x => x.GetConfigureForNativeCodeAttribute() != null);
        var runtimeMonikerProtections = allProtections
            .Select(x => (x, x.GetRuntimeMonikerAttributes()))
            .Where(x => x.Item2.Any());

        var il2cppExcludedProtections = il2cppIncompatibleProtections
            .Select(x => (Protection: x, Reason: x.GetIL2CPPIncompatibleReason()))
            .ToList();

        var hasProtections = !sortedProtections.IsEmpty() || !packers.IsEmpty();

        return new ProtectionsSort(
            protectionsResolve,
            allProtections.ToList(),
            sortedProtections.ToList(),
            pipelineProtections.ToList(),
            packers.ToList(),
            obfuscationAttributeProtections.ToList(),
            deprecatedProtections.ToList(),
            configureForNativeCodeProtections.ToList(),
            runtimeMonikerProtections.ToList(),
            il2cppExcludedProtections,
            hasProtections);
    }
}