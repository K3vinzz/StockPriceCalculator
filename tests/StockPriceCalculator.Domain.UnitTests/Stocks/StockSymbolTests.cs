using StockPriceCalculator.Domain.Stocks;

namespace StockPriceCalculator.Domain.UnitTests.Stocks;

public class StockSymbolTests
{
    [Fact]
    public void ctor_ShouldTrimAndUppercaseValue()
    {
        var symbol = new StockSymbol("  2330  ");

        Assert.Equal("2330", symbol.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ctor_GivenEmptyOrWhitespace_ShouldThrow(string input)
    {
        Assert.Throws<ArgumentException>(() => new StockSymbol(input));
    }
}

