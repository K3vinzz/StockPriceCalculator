using Microsoft.EntityFrameworkCore;
using StockPriceCalculator.Domain.Stocks;
using StockPriceCalculator.Domain.Stocks.Interfaces;
using StockPriceCalculator.Infrastructure.Persistence;
using StockPriceCalculator.Infrastructure.Persistence.Entities;

namespace StockPriceCalculator.Infrastructure.Repositories;

public class StockInfoRepository : IStockInfoRepository
{
    private readonly StockDbContext _db;

    public StockInfoRepository(StockDbContext stockDbContext)
    {
        _db = stockDbContext;
    }

    public async Task AddStocksAsync(IEnumerable<Stock> stocks, string market)
    {
        var entities = stocks.Select(s => new StockEntity
        {
            Symbol = s.Symbol,
            Name = s.Name,
            Market = market
        });

        await _db.Stocks.AddRangeAsync(entities);
        await _db.SaveChangesAsync();
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

    public async Task TruncateStockListAsync()
    {
        using var tx = await _db.Database.BeginTransactionAsync();

        await _db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE stocks");

        await _db.SaveChangesAsync();

        await tx.CommitAsync();
    }
}
