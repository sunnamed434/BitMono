using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BitMono.API.Protections;
using BitMono.Core.Attributes;
using BitMono.Core.Extensions;
using BitMono.Core.Resolvers;
using BitMono.Obfuscation.Protections;
using BitMono.Shared.Models;

namespace BitMono.Obfuscation.Tests.Protections;

public class ProtectionsSorterIL2CPPTests
{
    private static ProtectionsSorter CreateSorter()
    {
        var resolver = new ObfuscationAttributeResolver(new ObfuscationSettings());
        var assembly = new AssemblyDefinition("Test", new Version(1, 0, 0, 0));
        return new ProtectionsSorter(resolver, assembly);
    }

    private static List<ProtectionSetting> EnabledSettings(params string[] names)
    {
        var settings = new List<ProtectionSetting>();
        foreach (var name in names)
        {
            settings.Add(new ProtectionSetting { Name = name, Enabled = true });
        }
        return settings;
    }

    [Fact]
    public void IL2CPPMode_ExcludesIncompatibleProtections_AndKeepsCompatibleOnes()
    {
        var sorter = CreateSorter();
        var protections = new List<IProtection> { new SafeProtection(), new HookProtection(), new NativeProtection() };
        var settings = EnabledSettings(nameof(SafeProtection), nameof(HookProtection), nameof(NativeProtection));

        var sort = sorter.Sort(protections, settings, il2cpp: true);

        sort.SortedProtections.Select(p => p.GetName()).ShouldBe(new[] { nameof(SafeProtection) });
        sort.IL2CPPIncompatibleProtections.Select(x => x.Protection.GetName())
            .ShouldBe(new[] { nameof(HookProtection), nameof(NativeProtection) }, ignoreOrder: true);
        sort.HasProtections.ShouldBeTrue();
    }

    [Fact]
    public void WithoutIL2CPPMode_AllProtectionsRun_AndNothingIsReportedExcluded()
    {
        var sorter = CreateSorter();
        var protections = new List<IProtection> { new SafeProtection(), new HookProtection(), new NativeProtection() };
        var settings = EnabledSettings(nameof(SafeProtection), nameof(HookProtection), nameof(NativeProtection));

        var sort = sorter.Sort(protections, settings, il2cpp: false);

        sort.SortedProtections.Count.ShouldBe(3);
        sort.IL2CPPIncompatibleProtections.ShouldBeEmpty();
    }

    [Fact]
    public void IL2CPPMode_WhenEveryProtectionIsIncompatible_HasNoProtections()
    {
        var sorter = CreateSorter();
        var protections = new List<IProtection> { new HookProtection() };
        var settings = EnabledSettings(nameof(HookProtection));

        var sort = sorter.Sort(protections, settings, il2cpp: true);

        sort.HasProtections.ShouldBeFalse();
        sort.IL2CPPIncompatibleProtections.Count.ShouldBe(1);
    }

    private class SafeProtection : IProtection
    {
        public Task ExecuteAsync() => Task.CompletedTask;
    }

    [IL2CPPIncompatible("runtime hook, no JIT under IL2CPP")]
    private class HookProtection : IProtection
    {
        public Task ExecuteAsync() => Task.CompletedTask;
    }

    [ConfigureForNativeCode]
    private class NativeProtection : IProtection
    {
        public Task ExecuteAsync() => Task.CompletedTask;
    }
}
