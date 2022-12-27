﻿namespace BitMono.API.Protecting.Resolvers;

public interface IObfuscationAttributeExcludeResolver
{
    bool TryResolve(string feature, IHasCustomAttribute from, out Dictionary<string, CustomAttributesResolve> keyValuePairs);
}