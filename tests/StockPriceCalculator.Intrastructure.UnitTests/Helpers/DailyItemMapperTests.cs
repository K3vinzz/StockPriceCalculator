using System;
using StockPriceCalculator.Infrastructure.Helpers;
using StockPriceCalculator.Infrastructure.Services.Dtos;

namespace StockPriceCalculator.Intrastructure.UnitTests.Helpers;

public class DailyItemMapperTests
{
    [Fact]
    public void TryToDomain_ValidDto_ReturnsTrueAndMapsCorrectly()
    {
        // Arrange
        var dto = new TwseDailyItem
        {
            Date = "1140102",
            Code = "2330",
            Name = "台積電",
            ClosingPrice = "650.5",
            MonthlyAveragePrice = "640.0"
        };

        // Act
        var success = dto.TryToDomain(out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);

        // Symbol
        Assert.Equal("2330", result!.Symbol);

        var expectedDate = new DateOnly(2025, 1, 2);
        Assert.Equal(expectedDate, result.Date);

        Assert.Equal(650.5m, result.ClosePrice.Amount);
        Assert.Equal("TWD", result.ClosePrice.Currency);
    }

    [Fact]
    public void TryToDomain_InvalidDate_ReturnsFalseAndNullResult()
    {
        // Arrange
        var dto = new TwseDailyItem
        {
            Date = "invalid-date",
            Code = "2330",
            Name = "台積電",
            ClosingPrice = "650.5",
            MonthlyAveragePrice = "640.0"
        };

        // Act
        var success = dto.TryToDomain(out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryToDomain_InvalidPrice_ReturnsFalseAndNullResult()
    {
        // Arrange
        var dto = new TwseDailyItem
        {
            Date = "1140102",
            Code = "2330",
            Name = "台積電",
            ClosingPrice = "not-a-number",
            MonthlyAveragePrice = "640.0"
        };

        // Act
        var success = dto.TryToDomain(out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }
}