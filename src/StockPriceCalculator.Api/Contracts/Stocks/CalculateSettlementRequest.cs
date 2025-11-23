namespace StockPriceCalculator.Api.Contracts.Stocks;

public record class CalculateSettlementRequest(string Symbol, string Date, int Quantity, string Market);
