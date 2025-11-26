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

    public async Task<Stock?> MatchStockAsync(string keyword)
    {
        var entity = await _db.Stocks
            .FirstOrDefaultAsync(x => x.Symbol == keyword);

        if (entity is null)
        {
            return null;
        }

        return new Stock(symbol: entity.Symbol, name: entity.Name, market: entity.Market);
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
