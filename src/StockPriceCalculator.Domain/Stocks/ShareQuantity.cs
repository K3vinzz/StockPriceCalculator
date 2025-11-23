namespace StockPriceCalculator.Domain.Stocks;

public sealed record ShareQuantity
{
    public int Value { get; }

    public ShareQuantity(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Share quantity must be greater than zero.", nameof(value));

        Value = value;
    }

    public static implicit operator int(ShareQuantity q) => q.Value;
}
