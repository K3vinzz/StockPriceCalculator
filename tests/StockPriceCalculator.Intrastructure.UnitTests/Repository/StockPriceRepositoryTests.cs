using System;
using Microsoft.EntityFrameworkCore;
using StockPriceCalculator.Domain.Primitives;
using StockPriceCalculator.Domain.Stocks;
using StockPriceCalculator.Infrastructure.Persistence;
using StockPriceCalculator.Infrastructure.Persistence.Entities;
using StockPriceCalculator.Infrastructure.Repositories;

namespace StockPriceCalculator.Intrastructure.UnitTests.Repository;

public class StockPriceRepositoryTests
{
    private static StockDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new StockDbContext(options);
    }

    [Fact]
    public async Task GetDailyStockPricesByMonth_ShouldReturnTargetStockPriceAndMonth()
    {
        using var context = CreateDbContext();

        // 準備測試資料
        context.DailyStockPrices.AddRange(new[]
        {
            new DailyStockPriceEntity
            {
                Symbol = "2330",
                TradeDate = new DateOnly(2025, 11, 1),
                ClosePrice = 100m,
                Market = "TWSE",
                Name = "台積電",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new DailyStockPriceEntity
            {
                Symbol = "2330",
                TradeDate = new DateOnly(2025, 11, 15),
                ClosePrice = 110m,
                Market = "TWSE",
                Name = "台積電",
                CreatedAt = DateTimeOffset.UtcNow
            },

            // 目標股票但不同月份（不應該被查到）
            new DailyStockPriceEntity
            {
                Symbol = "2330",
                TradeDate = new DateOnly(2025, 10, 31),
                ClosePrice = 90m,
                Market = "TWSE",
                Name = "台積電",
                CreatedAt = DateTimeOffset.UtcNow
            },

            // 不同股票同月份（不應該被查到）
            new DailyStockPriceEntity
            {
                Symbol = "2317",
                TradeDate = new DateOnly(2025, 11, 1),
                ClosePrice = 50m,
                Market = "TWSE",
                Name = "鴻海",
                CreatedAt = DateTimeOffset.UtcNow
            }
        });

        await context.SaveChangesAsync();

        var repository = new StockPriceRepository(context);
        var symbol = new StockSymbol("2330");
        var anyDayInNovember = new DateOnly(2025, 11, 20);

        // Act
        var result = await repository.GetDailyStockPricesByMonth(symbol, anyDayInNovember);

        // Assert
        // 1. 只抓到 2 筆（11/1、11/15）
        Assert.Equal(2, result.Count);

        // 2. 全部都是指定股票 2330
        Assert.All(result, x => Assert.Equal(symbol, x.Symbol));

        // 3. 全部都在 2025/11 這個月份
        Assert.All(result, x =>
        {
            Assert.Equal(2025, x.Date.Year);
            Assert.Equal(11, x.Date.Month);
        });

        // 4. 價格有抓到 100 和 110
        var amounts = new HashSet<decimal> { 100m, 110m };
        Assert.All(result, x =>
        {
            Assert.Contains(x.ClosePrice.Amount, amounts);
            Assert.Equal("TWD", x.ClosePrice.Currency);
            Assert.Equal("台積電", x.Name);
        });
    }

    [Fact]
    public async Task AddDailyStockPrices_Should_Insert_All_Records_With_Correct_Mapping()
    {
        // Arrange
        using var context = CreateDbContext();

        var repository = new StockPriceRepository(context);

        var symbol = new StockSymbol("2330");
        var anotherSymbol = new StockSymbol("2317");

        // 你這邊依照你實際的 DailyStockPrice 建構子去 new
        // 我示意用一個 (symbol, date, money, name, market) 的版本
        var prices = new List<DailyStockPrice>
        {
            new DailyStockPrice(
                symbol: symbol,
                date: new DateOnly(2025, 11, 3),
                closePrice: new Money(600m, "TWD"),
                name: "台積電",
                market: "TWSE"
            ),
            new DailyStockPrice(
                symbol: symbol,
                date: new DateOnly(2025, 11, 4),
                closePrice: new Money(610m, "TWD"),
                name: "台積電",
                market: "TWSE"
            ),
            new DailyStockPrice(
                symbol: anotherSymbol,
                date: new DateOnly(2025, 11, 3),
                closePrice: new Money(120m, "TWD"),
                name: "鴻海",
                market: "TWSE"
            )
        };

        // Act
        await repository.AddDailyStockPrices(prices);

        // Assert
        var entities = await context.DailyStockPrices
            .AsNoTracking()
            .ToListAsync();

        // 1. 總數要 match
        Assert.Equal(3, entities.Count);

        // 2. 檢查第一筆（台積電 2025-11-03）
        var tsmc1103 = Assert.Single(
            entities,
            e => e.Symbol == "2330" && e.TradeDate == new DateOnly(2025, 11, 3));

        Assert.Equal("台積電", tsmc1103.Name);
        Assert.Equal(600m, tsmc1103.ClosePrice);
        Assert.Equal("TWSE", tsmc1103.Market);

        // 3. 檢查第二筆（台積電 2025-11-04）
        var tsmc1104 = Assert.Single(
            entities,
            e => e.Symbol == "2330" && e.TradeDate == new DateOnly(2025, 11, 4));

        Assert.Equal("台積電", tsmc1104.Name);
        Assert.Equal(610m, tsmc1104.ClosePrice);
        Assert.Equal("TWSE", tsmc1104.Market);

        // 4. 檢查第三筆（2317）
        var honhai1103 = Assert.Single(
            entities,
            e => e.Symbol == "2317" && e.TradeDate == new DateOnly(2025, 11, 3));

        Assert.Equal("鴻海", honhai1103.Name);
        Assert.Equal(120m, honhai1103.ClosePrice);
        Assert.Equal("TWSE", honhai1103.Market);
    }
}
