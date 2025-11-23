namespace StockPriceCalculator.Application.Stocks.Commands;

public sealed record CalculateSettlementResult(
    string Symbol,
    DateOnly TradeDate,
    int Shares,
    decimal ClosePrice = 0,
    decimal TotalAmount = 0
);
