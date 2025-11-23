using Microsoft.EntityFrameworkCore;
using StockPriceCalculator.Domain.Primitives;
using StockPriceCalculator.Domain.Stocks;
using StockPriceCalculator.Domain.Stocks.Interfaces;
using StockPriceCalculator.Infrastructure.Persistence;
using StockPriceCalculator.Infrastructure.Persistence.Entities;

namespace StockPriceCalculator.Infrastructure.Repositories;

public class StockPriceRepository : IStockPriceRepository
{
    private readonly StockDbContext _db;

    public StockPriceRepository(StockDbContext db)
    {
        _db = db;
    }

    public async Task AddDailyStockPrices(IEnumerable<DailyStockPrice> dailyStockPrices)
    {
        var entities = dailyStockPrices.Select(x => new DailyStockPriceEntity
        {
            Symbol = x.Symbol,
            Name = x.Name,
            TradeDate = x.Date,
            ClosePrice = x.ClosePrice.Amount,
            Market = x.Market
        });

        await _db.DailyStockPrices.AddRangeAsync(entities);
        await _db.SaveChangesAsync();
    }

    public async Task AddStocksAsync(IEnumerable<Stock> stocks)
    {
        var entities = stocks.Select(s =>
        {
            var marketCode = s.Market switch
            {
                "上市" => "TWSE",
                "上櫃" => "TPEX",
                "興櫃" => "Emerging",
                _ => "unknown"
            };

            return new StockEntity
            {
                Symbol = s.Symbol,
                Name = s.Name,
                Market = marketCode
            };
        });

        await _db.Stocks.AddRangeAsync(entities);
        await _db.SaveChangesAsync();
    }

    public async Task<DailyStockPrice?> GetDailyStockPriceAsync(string symbol, DateOnly date)
    {
        var entity = await _db.DailyStockPrices
            .AsNoTracking()
            .Where(x => x.Symbol == symbol && x.TradeDate == date)
            .FirstOrDefaultAsync();

        if (entity is null)
        {
            return null!;
        }

        return new DailyStockPrice(
            name: entity.Name,
            symbol: symbol,
            date: date,
            closePrice: new Money(entity.ClosePrice)
        );
    }

    public async Task<List<DailyStockPrice>> GetDailyStockPricesByMonth(string symbol, DateOnly date)
    {
        var startDate = new DateOnly(date.Year, date.Month, 1);
        var endDate = startDate.AddMonths(1);

        var result = await _db.DailyStockPrices
            .AsNoTracking()
            .Where(x => x.TradeDate >= startDate && x.TradeDate < endDate && x.Symbol == symbol)
            .Select(x => new DailyStockPrice(symbol, x.TradeDate, new Money(x.ClosePrice, "TWD"), x.Name, x.Market))
            .ToListAsync();

        return result;
    }
}
