namespace StockPriceCalculator.Infrastructure.Services.Dtos;

public sealed class TwseDailyItem
{
    public string Date { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string ClosingPrice { get; set; } = default!;
    public string MonthlyAveragePrice { get; set; } = default!;
}
