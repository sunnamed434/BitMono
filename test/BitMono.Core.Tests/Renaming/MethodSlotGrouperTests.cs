using BitMono.Core.Renaming;
using BitMono.Utilities.AsmResolver;

namespace BitMono.Core.Tests.Renaming;

public class MethodSlotGrouperTests
{
    private static (ModuleDefinition module, IReadOnlyList<MethodSlot> slots) Group()
    {
        var module = ModuleDefinition.FromFile(typeof(ISlotShape).Assembly.Location);
        var slots = new MethodSlotGrouper().Group(module);
        return (module, slots);
    }

    private static MethodDefinition Method(ModuleDefinition module, string typeName, Func<MethodDefinition, bool> predicate)
    {
        return module.GetAllTypes().First(t => t.Name == typeName).Methods.First(predicate);
    }

    private static MethodSlot SlotOf(IReadOnlyList<MethodSlot> slots, MethodDefinition method)
    {
        return slots.First(slot => slot.Methods.Contains(method));
    }

    [Fact]
    public void GroupsInterfaceMethodWithImplicitImplementation()
    {
        var (module, slots) = Group();
        var interfaceMethod = Method(module, nameof(ISlotShape), m => m.Name == nameof(ISlotShape.SlotDraw));
        var implementation = Method(module, nameof(SlotCircle), m => m.Name == nameof(SlotCircle.SlotDraw));

        var slot = SlotOf(slots, interfaceMethod);

        slot.Methods.Should().Contain(implementation);
        slot.TiedToExternal.Should().BeFalse();
    }

    [Fact]
    public void GroupsExplicitInterfaceImplementation()
    {
        var (module, slots) = Group();
        var interfaceMethod = Method(module, nameof(ISlotShape), m => m.Name == nameof(ISlotShape.SlotDraw));
        var explicitImplementation = Method(module, nameof(SlotSquare), m => m.Name?.Value?.EndsWith("SlotDraw") == true);

        SlotOf(slots, interfaceMethod).Methods.Should().Contain(explicitImplementation);
    }

    [Fact]
    public void GroupsVirtualOverrideChain()
    {
        var (module, slots) = Group();
        var baseMethod = Method(module, nameof(SlotAnimal), m => m.Name == nameof(SlotAnimal.SlotSpeak));
        var overrideMethod = Method(module, nameof(SlotDog), m => m.Name == nameof(SlotDog.SlotSpeak));

        var slot = SlotOf(slots, baseMethod);

        slot.Methods.Should().Contain(overrideMethod);
        slot.TiedToExternal.Should().BeFalse();
    }

    [Fact]
    public void KeepsOverrideOfExternalMethod()
    {
        var (module, slots) = Group();
        var toString = Method(module, nameof(SlotWidget), m => m.Name == "ToString");

        SlotOf(slots, toString).TiedToExternal.Should().BeTrue();
    }

    [Fact]
    public void GroupsGenericInterfaceImplementation()
    {
        var (module, slots) = Group();
        var interfaceMethod = Method(module, "ISlotBox`1", m => m.Name == "SlotGet");
        var implementation = Method(module, nameof(SlotIntBox), m => m.Name == nameof(SlotIntBox.SlotGet));

        var slot = SlotOf(slots, interfaceMethod);

        slot.Methods.Should().Contain(implementation);
        slot.TiedToExternal.Should().BeFalse();
    }
}
