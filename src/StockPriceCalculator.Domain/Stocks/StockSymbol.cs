namespace StockPriceCalculator.Domain.Stocks;

public sealed record StockSymbol
{
    public string Value { get; }

    public StockSymbol(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Stock symbol cannot be empty.", nameof(value));

        Value = value.Trim().ToUpperInvariant();
    }

    public override string ToString() => Value;
}