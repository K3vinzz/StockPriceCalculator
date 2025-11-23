using System.Globalization;
using StockPriceCalculator.Domain.Primitives;
using StockPriceCalculator.Domain.Stocks;
using StockPriceCalculator.Infrastructure.Services.Dtos;

namespace StockPriceCalculator.Infrastructure.Helpers;

public static class DailyItemMapper
{
    public static bool TryToDomain(this TwseDailyItem dto, out DailyStockPrice? result)
    {
        result = null;

        if (!DateHelper.TryParseRocDate(dto.Date, out var date))
        {
            return false;
        }

        if (!decimal.TryParse(dto.ClosingPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var close))
        {
            return false;
        }

        result = new DailyStockPrice(
            name: dto.Name,
            symbol: dto.Code,
            date: date,
            closePrice: new Money(close)
        );

        return true;
    }
}
