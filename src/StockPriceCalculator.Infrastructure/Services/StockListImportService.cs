using Microsoft.Extensions.Logging;
using StockPriceCalculator.Domain.Stocks.Interfaces;

namespace StockPriceCalculator.Infrastructure.Services;

public class StockListImportService
{
    private readonly IStockPriceRepository _stockPriceRepository;
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly ILogger<StockListImportService> _logger;

    public StockListImportService(IStockPriceRepository stockPriceRepository, IMarketDataProvider marketDataProvider, ILogger<StockListImportService> logger)
    {
        _stockPriceRepository = stockPriceRepository;
        _marketDataProvider = marketDataProvider;
        _logger = logger;
    }

    public async Task ImportStockListAsync()
    {
        var endpoints = new string[] { "IsinTwseEndpoint", "IsinTpexEndpoint1", "IsinTpexEndpoint2" };

        foreach (var endpoint in endpoints)
        {
            var list = await _marketDataProvider.FetchAllStocks(endpoint);

            if (list is null || list.Count == 0)
            {
                _logger.LogWarning("Stock List is empty: {endpoint}", endpoint);
                return;
            }

            await _stockPriceRepository.AddStocksAsync(list);
        }
    }
}
