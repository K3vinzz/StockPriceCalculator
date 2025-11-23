using System;
using StockPriceCalculator.Domain.Stocks;

namespace StockPriceCalculator.Domain.UnitTests.Stocks;

public class ShareQuantityTests
{
    [Fact]
    public void ctor_ShouldStoreValue()
    {
        var quantity = new ShareQuantity(100);
        Assert.Equal(100, quantity.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ctor_WhenValueIsNotPositive_ShouldThrow(int value)
    {
        Assert.Throws<ArgumentException>(() => new ShareQuantity(value));
    }

    [Fact]
    public void ImplicitConversion_ToInt_ShouldWork()
    {
        ShareQuantity quantity = new(50);
        int value = quantity;

        Assert.Equal(50, value);
    }
}

