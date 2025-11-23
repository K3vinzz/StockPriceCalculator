using NSubstitute;
using NSubstitute.ReturnsExtensions;
using StockPriceCalculator.Application.Stocks.Commands;
using StockPriceCalculator.Application.Stocks.Handlers;
using StockPriceCalculator.Domain.Abstractions;
using StockPriceCalculator.Domain.Primitives;
using StockPriceCalculator.Domain.Stocks;
using StockPriceCalculator.Domain.Stocks.Interfaces;

namespace StockPriceCalculator.Application.UnitTests;

public class CalculateSettlementHandlerTests
{
    private IStockPriceRepository _priceRepo;
    private IStockValuationRecordRepository _recordRepo;
    private IMarketDataProvider _marketDataProvider;
    private IClock _clock;
    private CalculateSettlementHandler _handler;

    public CalculateSettlementHandlerTests()
    {
        _priceRepo = Substitute.For<IStockPriceRepository>();
        _recordRepo = Substitute.For<IStockValuationRecordRepository>();
        _marketDataProvider = Substitute.For<IMarketDataProvider>();
        _clock = Substitute.For<IClock>();

        _handler = new CalculateSettlementHandler(
            _priceRepo,
            _recordRepo,
            _marketDataProvider,
            _clock
        );
    }

    [Fact]
    public async Task HandleAsync_WhenDailyPriceExists_ShouldUseItAndNotFetchFromMarket()
    {
        // Given
        var command = new CalculateSettlementCommand("2330", new DateOnly(2025, 11, 20), 100, "TWSE");
        var symbol = command.Symbol;
        var tradeDate = command.TradeDate;
        _priceRepo
            .GetDailyStockPriceAsync(symbol, tradeDate)
            .Returns(new DailyStockPrice(
                symbol,
                tradeDate,
                new Money(1000m),
                "台積電",
                "TWSE"
            ));

        _clock.UtcNow.Returns(new DateTimeOffset(2025, 11, 21, 0, 0, 0, TimeSpan.Zero));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        await _marketDataProvider.DidNotReceive()
            .GetTwseDaily(default!, default!);

        // await _recordRepo.Received(1).AddAsync(Arg.Any<StockValuationRecord>(), Arg.Any<CancellationToken>());

        Assert.Equal("2330", result.Symbol);
    }

    [Fact]
    public async Task HandleAsync_WhenPriceNotExists_FetchFromMarketAndAddToDb()
    {
        // Given
        var command = new CalculateSettlementCommand("2330", new DateOnly(2025, 11, 20), 100, "TWSE");
        var symbol = command.Symbol;
        var tradeDate = command.TradeDate;

        _priceRepo
            .GetDailyStockPriceAsync(symbol, tradeDate)
            .ReturnsNull();

        _marketDataProvider
            .GetTwseDaily(symbol, tradeDate)
            .Returns(
            [
                new(
                    symbol,
                    new DateOnly(2025, 11, 17),
                    new Money(100),
                    "台積電",
                    "TWSE"),
                new(
                    symbol,
                    new DateOnly(2025, 11, 18),
                    new Money(200),
                    "台積電",
                    "TWSE"),
                new(
                    symbol,
                    new DateOnly(2025, 11, 19),
                    new Money(300),
                    "台積電",
                    "TWSE"),
                new(
                    symbol,
                    new DateOnly(2025, 11, 20),
                    new Money(400),
                    "台積電",
                    "TWSE"),
                new(
                    symbol,
                    new DateOnly(2025, 11, 21),
                    new Money(500),
                    "台積電",
                    "TWSE"),
            ]);

        _priceRepo
            .GetDailyStockPricesByMonth(symbol, tradeDate)
            .Returns([]);

        _clock.UtcNow.Returns(new DateTimeOffset(2025, 11, 21, 0, 0, 0, TimeSpan.Zero));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        await _marketDataProvider.Received(1).GetTwseDaily(Arg.Any<string>(), Arg.Any<DateOnly>());

        // await _recordRepo.Received(1).AddAsync(Arg.Any<StockValuationRecord>(), Arg.Any<CancellationToken>());

        Assert.Equal("2330", result.Symbol);
        Assert.Equal(40000, result.TotalAmount);
    }

    [Fact]
    public async Task HandleAsync_WhenPartsOfPriceExists_FetchFromMarketAndAddNotExistingDataToDb()
    {
        // Given
        var command = new CalculateSettlementCommand("2330", new DateOnly(2025, 11, 20), 100, "TWSE");
        var symbol = command.Symbol;
        var tradeDate = command.TradeDate;

        _priceRepo
            .GetDailyStockPriceAsync(symbol, tradeDate)
            .ReturnsNull();

        _marketDataProvider
            .GetTwseDaily(symbol, tradeDate)
            .Returns(
            [
                new(
                    symbol,
                    new DateOnly(2025, 11, 17),
                    new Money(100),
                    "台積電",
                    "TWSE"),
                new(
                    symbol,
                    new DateOnly(2025, 11, 18),
                    new Money(200),
                    "台積電",
                    "TWSE"),
                new(
                    symbol,
                    new DateOnly(2025, 11, 19),
                    new Money(300),
                    "台積電",
                    "TWSE"),
                new(
                    symbol,
                    new DateOnly(2025, 11, 20),
                    new Money(400),
                    "台積電",
                    "TWSE"),
                new(
                    symbol,
                    new DateOnly(2025, 11, 21),
                    new Money(500),
                    "台積電",
                    "TWSE"),
            ]);

        _priceRepo
            .GetDailyStockPricesByMonth(symbol, tradeDate)
            .Returns([
                new(
                    symbol,
                    new DateOnly(2025, 11, 17),
                    new Money(100),
                    "台積電",
                    "TWSE"),
                new(
                    symbol,
                    new DateOnly(2025, 11, 18),
                    new Money(200),
                    "台積電",
                    "TWSE")
            ]);

        _clock.UtcNow.Returns(new DateTimeOffset(2025, 11, 21, 0, 0, 0, TimeSpan.Zero));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        await _marketDataProvider.Received(1).GetTwseDaily(Arg.Any<string>(), Arg.Any<DateOnly>());

        // await _recordRepo.Received(1).AddAsync(Arg.Any<StockValuationRecord>(), Arg.Any<CancellationToken>());

        await _priceRepo
            .Received(1)
            .AddDailyStockPrices(Arg.Is<IEnumerable<DailyStockPrice>>(list => MatchExpectedPrices(list, symbol)));

        Assert.Equal("2330", result.Symbol);
        Assert.Equal(40000, result.TotalAmount);
    }

    private static bool MatchExpectedPrices(
    IEnumerable<DailyStockPrice> list,
    string symbol)
    {
        var items = list.ToList();
        if (items.Count != 3) return false;

        var expected = new Dictionary<DateOnly, decimal>
        {
            [new DateOnly(2025, 11, 19)] = 300m,
            [new DateOnly(2025, 11, 20)] = 400m,
            [new DateOnly(2025, 11, 21)] = 500m,
        };

        foreach (var item in items)
        {
            if (item.Symbol != symbol) return false;
            if (item.Market != "TWSE") return false;
            if (!expected.TryGetValue(item.Date, out var price)) return false;
            if (item.ClosePrice.Amount != price) return false;
        }

        return true;
    }
}
