namespace ECommerce.UnitTests.Bootstrap;

public sealed class BootstrapSmokeTests
{
    [Fact]
    public void Solution_bootstrap_should_reference_core_assembly()
    {
        var assemblyName = typeof(ECommerce.Core.Aggregates.AggregateRoot).Assembly.GetName().Name;

        Assert.Equal("ECommerce.Core", assemblyName);
    }
}
