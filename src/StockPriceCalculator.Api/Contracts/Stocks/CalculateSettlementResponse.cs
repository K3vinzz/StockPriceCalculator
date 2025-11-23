namespace StockPriceCalculator.Api.Contracts.Stocks;

public sealed record CalculateSettlementResponse(
    string Symbol,
    string TradeDate,
    int Shares,
    decimal ClosePrice,
    decimal TotalAmount,
    bool HasPriceData
);
