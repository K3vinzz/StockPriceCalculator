using StockPriceCalculator.Domain.Primitives;
using StockPriceCalculator.Domain.Stocks;

namespace StockPriceCalculator.Domain.UnitTests.Primitives;

public class MoneyTests
{
    [Fact]
    public void Multiply_ByShareQuantity_ShouldReturnNewMoney()
    {
        var price = new Money(10.5m, "TWD");
        var qty = new ShareQuantity(100);

        var total = price * qty;

        Assert.Equal(1050m, total.Amount);
        Assert.Equal("TWD", total.Currency);
    }
}
