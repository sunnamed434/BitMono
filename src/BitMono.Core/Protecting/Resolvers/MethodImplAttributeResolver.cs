﻿namespace BitMono.Core.Protecting.Resolvers;

public class MethodImplAttributeResolver : AttributeResolver
{
    private readonly IConfiguration m_Configuration;
    private readonly IAttemptAttributeResolver m_AttemptAttributeResolver;

    public MethodImplAttributeResolver(IBitMonoObfuscationConfiguration configuration, IAttemptAttributeResolver attemptAttributeResolver) 
    {
        m_Configuration = configuration.Configuration;
        m_AttemptAttributeResolver = attemptAttributeResolver;
    }

    public override bool Resolve([AllowNull] string feature, IHasCustomAttribute from, [AllowNull] out CustomAttributeResolve attributeResolve)
    {
        attributeResolve = null;
        if (m_Configuration.GetValue<bool>(nameof(Obfuscation.NoInliningMethodObfuscationExclude)) == false)
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve(from, typeof(MethodImplAttribute), out Dictionary<string, CustomAttributeResolve> keyValuePairs) == false)
        {
            return false;
        }
        if (keyValuePairs.TryGetValue(nameof(MethodImplAttribute.Value), out attributeResolve) == false)
        {
            return false;
        }
        if (attributeResolve.Value is MethodImplOptions valueValue)
        {
            if (valueValue.HasFlag(MethodImplOptions.NoInlining))
            {
                return true;
            }
        }
        return false;
    }
}