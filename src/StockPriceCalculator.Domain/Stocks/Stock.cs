namespace StockPriceCalculator.Domain.Stocks;

public class Stock
{
    public string Symbol { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Market { get; private set; } = default!;

    private Stock() { }

    public Stock(string symbol, string name, string market)
    {
        Symbol = symbol;
        Name = name;
        Market = market;
    }
}

