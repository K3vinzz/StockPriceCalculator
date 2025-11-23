namespace StockPriceCalculator.Application.Stocks.Commands;

public sealed record CalculateSettlementCommand
(
    string Symbol,
    DateOnly TradeDate,
    int Shares,
    string Market
);
