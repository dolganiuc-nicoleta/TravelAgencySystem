using TravelAgency.Core.Patterns.Strategy;

namespace TravelAgency.Tests;

public class PricingStrategyTests
{
    [Fact]
    public void Standard_AddsExtrasToBasePrice()
    {
        var context = new PricingContext(new StandardPricingStrategy());

        Assert.Equal(1150m, context.CalculateFinalPrice(1000m, 150m));
    }

    [Fact]
    public void Discount_AppliesPercentDiscountBeforeExtras()
    {
        var context = new PricingContext(new DiscountPricingStrategy(10m));

        Assert.Equal(950m, context.CalculateFinalPrice(1000m, 50m));
    }

    [Fact]
    public void Full_AppliesDiscountThenVatThenExtras()
    {
        // 1000 - 10% = 900; +19% VAT = 1071; +50 extras = 1121
        var context = new PricingContext(new FullPricingStrategy(10m, 19m));

        Assert.Equal(1121m, context.CalculateFinalPrice(1000m, 50m));
    }

    [Fact]
    public void Context_UsesStrategySwappedAtRuntime()
    {
        var context = new PricingContext(new StandardPricingStrategy());
        context.SetStrategy(new DiscountPricingStrategy(50m));

        Assert.Equal(500m, context.CalculateFinalPrice(1000m));
    }
}
