namespace StockPriceCalculator.Domain.Stocks;

public class Stock
{
    public StockSymbol Symbol { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Market { get; private set; } = default!;

    private Stock() { }

    public Stock(StockSymbol symbol, string name, string market)
    {
        Symbol = symbol;
        Name = name;
        Market = market;
    }

    public void Update(string name, string market)
    {
        Name = name;
        Market = market;
    }
}

