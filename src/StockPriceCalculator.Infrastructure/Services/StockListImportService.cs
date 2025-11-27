using Microsoft.Extensions.Logging;
using StockPriceCalculator.Domain.Stocks.Interfaces;

namespace StockPriceCalculator.Infrastructure.Services;

public class StockListImportService
{
    private const string IsinTwseEndpoint = "isin/C_public.jsp?strMode=2"; // 上市
    private const string IsinTpexEndpoint = "isin/C_public.jsp?strMode=4"; // 上櫃
    private readonly IStockInfoRepository _stockInfoRepository;
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly ILogger<StockListImportService> _logger;

    public StockListImportService(
        IStockInfoRepository stockInfoRepository,
        IMarketDataProvider marketDataProvider,
        ILogger<StockListImportService> logger)
    {
        _stockInfoRepository = stockInfoRepository;
        _marketDataProvider = marketDataProvider;
        _logger = logger;
    }

    public async Task RefreshStockListAsync()
    {
        try
        {
            await _stockInfoRepository.TruncateStockListAsync();
            _logger.LogInformation("Truncated stock list successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to truncate stock list.");
            return;
        }

        var endpoints = new string[] { IsinTwseEndpoint, IsinTpexEndpoint };

        foreach (var endpoint in endpoints)
        {
            var list = await _marketDataProvider.FetchAllStocks(endpoint);

            if (list is null || list.Count == 0)
            {
                _logger.LogWarning("Stock List is empty: {endpoint}", endpoint);
                return;
            }

            var market = endpoint switch
            {
                IsinTwseEndpoint => "TWSE",
                IsinTpexEndpoint => "TPEX",
                _ => "unknown"
            };

            await _stockInfoRepository.AddStocksAsync(list, market);
        }
    }
}
