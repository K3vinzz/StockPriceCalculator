using System;

namespace StockPriceCalculator.Infrastructure.External;

public class TwseStockDayAvgDto
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string ClosingPrice { get; set; } = default!;
    public string MonthlyAveragePrice { get; set; } = default!;
}
