namespace StockPriceCalculator.Domain.Stocks.Interfaces;

public interface IStockInfoRepository
{
    Task<List<Stock>> SearchStocksAsync(string keyword);
}
