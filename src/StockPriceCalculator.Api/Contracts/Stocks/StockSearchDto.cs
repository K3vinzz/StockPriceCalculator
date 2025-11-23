namespace StockPriceCalculator.Api.Contracts.Stocks;

public sealed record StockSearchDto(string Symbol, string Name, string Market);
