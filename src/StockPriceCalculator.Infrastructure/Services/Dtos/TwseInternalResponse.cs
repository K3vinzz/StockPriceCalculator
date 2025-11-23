namespace StockPriceCalculator.Infrastructure.Services.Dtos;

public class TwseStockDayResponse
{
    public string Stat { get; set; } = default!;
    public string Date { get; set; } = default!;
    public string Title { get; set; } = default!;
    public List<string> Fields { get; set; } = default!;
    public List<List<string>> Data { get; set; } = default!;
    public List<string>? Notes { get; set; }
    public int Total { get; set; }
}
