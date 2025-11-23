using StockPriceCalculator.Infrastructure.Helpers;

namespace StockPriceCalculator.Intrastructure.UnitTests.Helpers;

public class DateHelperTests
{
    [Fact]
    public void TryParseRocDate_ShouldReturnCorrectDate()
    {
        // Given
        string rocDate = "1141120";

        // When
        var res = DateHelper.TryParseRocDate(rocDate, out var date);

        // Then
        Assert.True(res);
        Assert.Equal<DateOnly>(new DateOnly(2025, 11, 20), date);
    }

    [Theory]
    [InlineData("")]
    [InlineData(".  ")]
    [InlineData("123456789")]
    [InlineData("123")]
    [InlineData("kevin")]
    public void TryParseRocDate_GivenEmptyOrInvalidInputs_ShouldReturnFalseWithDefaultDate(string rocDate)
    {
        // When
        var res = DateHelper.TryParseRocDate(rocDate, out var date);
        // Then
        Assert.False(res);
        Assert.Equal<DateOnly>(new DateOnly(), date);
    }
}
