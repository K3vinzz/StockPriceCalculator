namespace StockPriceCalculator.Domain.Stocks.Interfaces;

public interface IMarketDataProvider
{
    Task<List<DailyStockPrice>> GetTwseDaily(string symbol, DateOnly date);
    Task<List<DailyStockPrice>> GetTpexDaily(string symbol, DateOnly date);
    Task<List<Stock>> FetchAllStocks(string endpoint);
}
