﻿namespace BitMono.API.Protecting.Resolvers;

public interface IMethodImplAttributeExcludeResolver
{
    bool TryResolve(IHasCustomAttribute from, out MethodImplAttribute obfuscationAttribute);
}