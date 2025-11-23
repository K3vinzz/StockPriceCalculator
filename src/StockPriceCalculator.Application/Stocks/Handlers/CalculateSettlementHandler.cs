using StockPriceCalculator.Application.Stocks.Commands;
using StockPriceCalculator.Domain.Abstractions;
using StockPriceCalculator.Domain.Stocks;
using StockPriceCalculator.Domain.Stocks.Interfaces;

namespace StockPriceCalculator.Application.Stocks.Handlers;

public class CalculateSettlementHandler
{
    private readonly IStockPriceRepository _priceRepository;
    private readonly IStockValuationRecordRepository _recordRepository;
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly IClock _clock;

    public CalculateSettlementHandler(
        IStockPriceRepository stockPriceRepository,
        IStockValuationRecordRepository stockValuationRecordRepository,
        IMarketDataProvider marketDataProvider,
        IClock clock)
    {
        _priceRepository = stockPriceRepository;
        _recordRepository = stockValuationRecordRepository;
        _marketDataProvider = marketDataProvider;
        _clock = clock;
    }

    public async Task<CalculateSettlementResult> HandleAsync(CalculateSettlementCommand command, CancellationToken ct)
    {
        var symbol = new StockSymbol(command.Symbol);
        var shares = new ShareQuantity(command.Shares);
        var tradeDate = command.TradeDate;
        var market = command.Market;

        var dailyPriceRecord = await _priceRepository.GetDailyStockPriceAsync(symbol, tradeDate, ct);

        if (dailyPriceRecord is null)
        {
            var fetchedDatas = await FetchDailyByMarketAsync(market, symbol, tradeDate);

            if (fetchedDatas.Count == 0)
            {
                return new CalculateSettlementResult(symbol.Value, tradeDate, shares);
            }

            await InsertDataToDb(symbol, tradeDate, fetchedDatas);

            dailyPriceRecord = fetchedDatas.SingleOrDefault(x => x.Date == tradeDate);
        }

        if (dailyPriceRecord is null)
        {
            return new CalculateSettlementResult(symbol.Value, tradeDate, shares);
        }

        var closePrice = dailyPriceRecord.ClosePrice;

        var record = StockValuationRecord.Create(
            symbol,
            command.TradeDate,
            shares,
            closePrice,
            _clock.UtcNow
        );

        Console.WriteLine($"Record: {record.Symbol.Value}, {record.Date}, {record.Shares}, {record.ClosePrice}, {record.CreatedAt}");

        // await _recordRepository.AddAsync(record, ct);

        return new CalculateSettlementResult(
            symbol.Value,
            command.TradeDate,
            shares.Value,
            closePrice.Amount,
            record.TotalAmount.Amount);
    }

    private async Task InsertDataToDb(StockSymbol symbol, DateOnly tradeDate, List<DailyStockPrice> fetchedDatas)
    {
        var existingDatas = await _priceRepository.GetDailyStockPricesByMonth(symbol, tradeDate);

        var existingDates = existingDatas.Select(x => x.Date).ToHashSet();

        var notExistingDatas = fetchedDatas.Where(x => !existingDates.Contains(x.Date));

        await _priceRepository.AddDailyStockPrices(notExistingDatas);
    }

    private async Task<List<DailyStockPrice>> FetchDailyByMarketAsync(
    string market,
    StockSymbol symbol,
    DateOnly tradeDate)
    {
        var normalizedMarket = market.ToLowerInvariant();

        return normalizedMarket switch
        {
            "twse" => await _marketDataProvider.GetTwseDaily(symbol.Value, tradeDate),
            "tpex" => await _marketDataProvider.GetTpexDaily(symbol.Value, tradeDate),
            _ => []
        };
    }
}
