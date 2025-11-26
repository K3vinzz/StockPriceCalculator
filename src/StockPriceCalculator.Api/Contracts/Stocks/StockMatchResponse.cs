using StockPriceCalculator.Domain.Stocks;

namespace StockPriceCalculator.Api.Contracts.Stocks;

public sealed record StockMatchResponse(Stock? Stock);
