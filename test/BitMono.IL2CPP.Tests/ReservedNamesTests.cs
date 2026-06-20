namespace BitMono.IL2CPP.Tests;

public class ReservedNamesTests
{
    [Theory]
    [InlineData("Awake")]
    [InlineData("Start")]
    [InlineData("Update")]
    [InlineData("OnTriggerEnter2D")]
    [InlineData(".ctor")]              // constructor
    [InlineData(".cctor")]            // static constructor
    [InlineData("System.IDisposable.Dispose")] // explicit interface impl
    [InlineData("System.Collections")] // dotted namespace string
    [InlineData("")]                  // empty (index 0)
    public void IsReserved_True_ForNamesTheEngineResolvesByText(string name)
    {
        ReservedNames.IsReserved(name).Should().BeTrue();
    }

    [Theory]
    [InlineData("PlayerController")]
    [InlineData("EnemyAI")]
    [InlineData("Inventory")]
    [InlineData("a")]
    [InlineData("Awakening")]         // not the magic method, just shares a prefix
    public void IsReserved_False_ForOrdinaryUserNames(string name)
    {
        ReservedNames.IsReserved(name).Should().BeFalse();
    }
}
