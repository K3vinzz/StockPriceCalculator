using Microsoft.EntityFrameworkCore;
using StockPriceCalculator.Domain.Stocks;
using StockPriceCalculator.Domain.Stocks.Interfaces;
using StockPriceCalculator.Infrastructure.Persistence;

namespace StockPriceCalculator.Infrastructure.Repositories;

public class StockInfoRepository : IStockInfoRepository
{
    private readonly StockDbContext _db;

    public StockInfoRepository(StockDbContext stockDbContext)
    {
        _db = stockDbContext;
    }
    public async Task<List<Stock>> SearchStocksAsync(string keyword)
    {
        return await _db.Stocks
            .Where(x => x.Symbol.Contains(keyword))
            .Take(20)
            .Select(x => new Stock(x.Symbol, x.Name, x.Market))
            .ToListAsync();
    }
}
