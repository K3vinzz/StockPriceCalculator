using System;

namespace StockPriceCalculator.Domain.Stocks.Interfaces;

public interface IStockPriceRepository
{
    Task<DailyStockPrice?> GetDailyStockPriceAsync(string symbol, DateOnly date);

    Task AddStocksAsync(IEnumerable<Stock> stocks, string market);

    Task<List<DailyStockPrice>> GetDailyStockPricesByMonth(string symbol, DateOnly date);

    Task AddDailyStockPrices(IEnumerable<DailyStockPrice> dailyStockPrices);
}
