using StockPriceCalculator.Domain.Stocks;

namespace StockPriceCalculator.Domain.Primitives;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "TWD")
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money operator *(Money money, ShareQuantity qty)
        => new Money(money.Amount * qty.Value, money.Currency);

    public override string ToString() => $"{Amount} {Currency}";
}