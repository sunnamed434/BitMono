using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BitMono.API.Protections;
using BitMono.Core.Extensions;
using BitMono.Core.Resolvers;
using BitMono.Obfuscation.Protections;
using BitMono.Shared.Models;

namespace BitMono.Obfuscation.Tests.Protections;

public class ProtectionsSorterTests
{
    private static ProtectionsSorter CreateSorter()
    {
        var resolver = new ObfuscationAttributeResolver(new ObfuscationSettings());
        var assembly = new AssemblyDefinition("Test", new Version(1, 0, 0, 0));
        return new ProtectionsSorter(resolver, assembly);
    }

    // A deprecated ([Obsolete]) protection must land in DeprecatedProtections, not in
    // ObfuscationAttributeExcludeProtections - otherwise the "Skip deprecated" and "Skip ... obfuscation
    // attribute" log lines are crossed.
    [Fact]
    public void DeprecatedProtection_GoesIntoDeprecatedBucket_NotObfuscationAttributeBucket()
    {
        var sorter = CreateSorter();
#pragma warning disable CS0612
        var deprecated = new DeprecatedDouble();
#pragma warning restore CS0612
        var protections = new List<IProtection> { new ActiveDouble(), deprecated };
        var settings = new List<ProtectionSetting>
        {
            new() { Name = nameof(ActiveDouble), Enabled = true },
            new() { Name = nameof(DeprecatedDouble), Enabled = true },
        };

        var sort = sorter.Sort(protections, settings);

        sort.DeprecatedProtections.Select(p => p.GetName()).Should().Equal(nameof(DeprecatedDouble));
        sort.ObfuscationAttributeExcludeProtections.Should().BeEmpty();
        sort.SortedProtections.Select(p => p.GetName()).Should().Equal(nameof(ActiveDouble));
    }

    private class ActiveDouble : IProtection
    {
        public Task ExecuteAsync() => Task.CompletedTask;
    }

    [Obsolete]
    private class DeprecatedDouble : IProtection
    {
        public Task ExecuteAsync() => Task.CompletedTask;
    }
}
