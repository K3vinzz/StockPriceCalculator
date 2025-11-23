using StockPriceCalculator.Domain.Primitives;

namespace StockPriceCalculator.Domain.Stocks;

public class DailyStockPrice
{
    public int Id { get; private set; }
    public string Name { get; set; } = default!;

    public StockSymbol Symbol { get; private set; } = default!;
    public DateOnly Date { get; private set; }

    public Money ClosePrice { get; private set; } = default!;
    public string Market { get; set; } = default!;

    // For EF Core materialization
    private DailyStockPrice() { }

    public DailyStockPrice(StockSymbol symbol, DateOnly date, Money closePrice, string name = "", string market = "unknown")
    {
        Symbol = symbol;
        Date = date;
        ClosePrice = closePrice;
        Name = name;
        Market = market;
    }
}
