using System;

namespace StockPriceCalculator.Domain.Stocks.Interfaces;

public interface IStockPriceRepository
{
    Task<DailyStockPrice?> GetDailyStockPriceAsync(StockSymbol symbol, DateOnly date, CancellationToken cancellationToken);

    Task AddStocksAsync(IEnumerable<Stock> stocks);

    Task<List<DailyStockPrice>> GetDailyStockPricesByMonth(StockSymbol symbol, DateOnly date);

    Task AddDailyStockPrices(IEnumerable<DailyStockPrice> dailyStockPrices);
}
