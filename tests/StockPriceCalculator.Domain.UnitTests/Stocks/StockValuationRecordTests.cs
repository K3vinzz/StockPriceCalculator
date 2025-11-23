using System;
using StockPriceCalculator.Domain.Primitives;
using StockPriceCalculator.Domain.Stocks;

namespace StockPriceCalculator.Domain.UnitTests.Stocks;

public class StockValuationRecordTests
{
    [Fact]
    public void Create_ShouldCalculateTotalAmount()
    {
        // arrange
        var symbol = "2330";
        var date = new DateOnly(2025, 1, 2);
        var shares = new ShareQuantity(1000);
        var close = new Money(610.5m, "TWD");
        var now = DateTime.Parse("2025-01-03T10:00:00+00:00");

        // act
        var record = StockValuationRecord.Create(symbol, date, shares, close, now);

        // assert
        Assert.Equal(symbol, record.Symbol);
        Assert.Equal(date, record.Date);
        Assert.Equal(shares, record.Shares);
        Assert.Equal(close, record.ClosePrice);
        Assert.Equal(610500m, record.TotalAmount.Amount);
        Assert.Equal(now, record.CreatedAt);
    }
}
