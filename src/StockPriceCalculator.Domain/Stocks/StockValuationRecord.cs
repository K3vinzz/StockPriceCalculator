using System;
using StockPriceCalculator.Domain.Primitives;

namespace StockPriceCalculator.Domain.Stocks;

public class StockValuationRecord
{
    public Guid Id { get; private set; }

    public StockSymbol Symbol { get; private set; } = default!;
    public DateOnly Date { get; private set; }
    public ShareQuantity Shares { get; private set; } = default!;

    public Money ClosePrice { get; private set; } = default!;
    public Money TotalAmount { get; private set; } = default!;

    public DateTimeOffset CreatedAt { get; private set; }

    private StockValuationRecord() { }

    private StockValuationRecord(
        StockSymbol symbol,
        DateOnly date,
        ShareQuantity shares,
        Money closePrice,
        DateTimeOffset createdAt)
    {
        Symbol = symbol;
        Date = date;
        Shares = shares;
        ClosePrice = closePrice;
        TotalAmount = new Money(closePrice.Amount * shares.Value, closePrice.Currency);
        CreatedAt = createdAt;
    }

    public static StockValuationRecord Create(
        StockSymbol symbol,
        DateOnly date,
        ShareQuantity shares,
        Money closePrice,
        DateTimeOffset now)
        => new(symbol, date, shares, closePrice, now);
}

