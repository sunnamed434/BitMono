using System.Text.RegularExpressions;

namespace BitMono.Core.Analyzing;

public class CriticalBaseTypesCriticalAnalyzer : ICriticalAnalyzer<TypeDefinition>
{
    private readonly CriticalsSettings _criticalsSettings;

    public CriticalBaseTypesCriticalAnalyzer(IOptions<CriticalsSettings> criticals)
    {
        _criticalsSettings = criticals.Value;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition type)
    {
        if (!_criticalsSettings.UseCriticalBaseTypes)
        {
            return true;
        }
        foreach (string ancestorFullName in YieldAncestors(type))
        {
            if (IsCriticalBaseType(ancestorFullName))
            {
                return false;
            }
        }
        return true;
    }

    private bool IsCriticalBaseType(string ancestorFullName)
    {
        ancestorFullName = StripGenerics(ancestorFullName);
        foreach (string criticalBaseType in _criticalsSettings.CriticalBaseTypes)
        {
            bool matches;
            if (criticalBaseType.Contains('*'))
            {
                string regex = "^" + Regex.Escape(criticalBaseType).Replace(@"\*", ".*") + "$";
                matches = Regex.IsMatch(ancestorFullName, regex);
            }
            else
            {
                matches = criticalBaseType == ancestorFullName;
            }
            if (matches)
            {
                return true;
            }
        }
        return false;
    }

    private static string StripGenerics(string fullName)
    {
        int genericDelimeterIndex = fullName.IndexOf('`');
        if (genericDelimeterIndex == -1)
        {
            return fullName;
        }
        return fullName[..genericDelimeterIndex];
    }

    /// <summary>
    /// Yields the full class names of all ancestors across the inheritence chain of <paramref name="current"/>, including itself.
    /// </summary>
    private static IEnumerable<string> YieldAncestors(TypeDefinition? current)
    {
        while (current != null)
        {
            yield return current.FullName;
            ITypeDefOrRef? baseType = current.BaseType;
            current = baseType?.Resolve();
            if (current == null && baseType != null)
            {
                //Type is in an unresolved dependency assembly, stop the search but yield the type name
                yield return baseType.FullName;
            }
        }
    }
}