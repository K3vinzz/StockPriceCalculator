using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using AngleSharp;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using StockPriceCalculator.Domain.Primitives;
using StockPriceCalculator.Domain.Stocks;
using StockPriceCalculator.Domain.Stocks.Interfaces;
using StockPriceCalculator.Infrastructure.Helpers;
using StockPriceCalculator.Infrastructure.Services.Dtos;

namespace StockPriceCalculator.Infrastructure.Services;

public class MarketDataProvider : IMarketDataProvider
{
    private const string TwseDailyEndpoint = "exchangeReport/STOCK_DAY_AVG_ALL";
    private const string TpexDailyEndpoint = "tpex_mainboard_quotes";
    private const string TwseDailyInternalEndpoint = "exchangeReport/STOCK_DAY";
    private const string TpexMainDailyInternalEndpoint = "www/zh-tw/afterTrading/tradingStock"; // 上櫃
    private const string TpexEmergingDailyInternalEndpoint = "www/zh-tw/emerging/historical"; // 興櫃

    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<IMarketDataProvider> _logger;

    public MarketDataProvider(IHttpClientFactory clientFactory, ILogger<IMarketDataProvider> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    /// <summary>
    /// 取得上市個股整月收盤價
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    public async Task<List<DailyStockPrice>> GetTwseDaily(string symbol, DateOnly date)
    {
        var result = new List<DailyStockPrice>();
        var client = _clientFactory.CreateClient("TWSE_internal");
        var url = QueryHelpers.AddQueryString(TwseDailyInternalEndpoint, new Dictionary<string, string?>
        {
            ["date"] = date.ToString("yyyyMMdd"),
            ["stockNo"] = symbol
        });

        TwseStockDayResponse? response;
        try
        {
            response = await client.GetFromJsonAsync<TwseStockDayResponse>(url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error calling TWSE API at {Endpoint}", TwseDailyInternalEndpoint);
            throw;
        }

        if (response is null)
        {
            _logger.LogWarning("TWSE internal endpoint returned empty result");
            return [];
        }

        if (!String.Equals(response.Stat, "ok", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Error when calling TWSE internal endpoint: {error}", response.Stat);
            return [];
        }

        foreach (var data in response.Data)
        {
            if (TryParseTwseInternalToDomain(data, symbol, out var domainModel))
            {
                result.Add(domainModel!);
            }
            else
            {
                _logger.LogWarning("Skipping invalid TWSE row: {@symbol}", $"{symbol}");
                continue;
            }
        }

        return result;
    }

    /// <summary>
    /// 取得上櫃個股整月收盤價
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    public async Task<List<DailyStockPrice>> GetTpexDaily(string symbol, DateOnly date)
    {
        var result = new List<DailyStockPrice>();
        var client = _clientFactory.CreateClient("TPEX_internal");
        var url = QueryHelpers.AddQueryString(TpexMainDailyInternalEndpoint, new Dictionary<string, string?>
        {
            ["date"] = date.ToString("yyyy/MM/dd"),
            ["code"] = symbol
        });

        TpexOuterResponse? response;
        try
        {
            response = await client.GetFromJsonAsync<TpexOuterResponse>(url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error calling TWSE API at {Endpoint}", url);
            throw;
        }

        if (response is null)
        {
            _logger.LogError("TPEX internal endpoint returned null response");
            return [];
        }

        if (!String.Equals(response.Stat, "ok", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("TPEX internal endpoint error: {Stat}", response.Stat);
            return [];
        }

        if (response.Tables == null || response.Tables.Count == 0)
        {
            _logger.LogWarning("TPEX internal endpoint returned OK but tables is empty");
            return [];
        }

        foreach (var data in response.Tables[0].Data)
        {
            if (TryParseTwseInternalToDomain(data, symbol, out var domainModel))
            {
                result.Add(domainModel!);
            }
            else
            {
                _logger.LogWarning("Skipping invalid TWSE row: {@symbol}", $"{symbol}");
                continue;
            }
        }

        return result;
    }

    public async Task<List<Stock>> FetchAllStocks(string endpoint)
    {
        var html = await FetchIsinHtml(endpoint);

        // await SaveHtmlToFile(html, "isin_tpex_1.txt");

        // Encoding big5Encoding = Encoding.GetEncoding(950);
        // var html = await File.ReadAllTextAsync("isin_twse.txt", big5Encoding);

        return ParseIsinHtml(html);
    }

    private async Task<string> FetchIsinHtml(string endpoint)
    {
        var client = _clientFactory.CreateClient("Isin.twse");

        var response = await client.GetAsync(endpoint);

        response.EnsureSuccessStatusCode();

        var responseBytes = await response.Content.ReadAsByteArrayAsync();

        // 2. 使用已知的 Big5 (MS950) 编码进行手动解码 (Code Page 950)
        // 假设您已在 Program.cs 注册了 CodePagesEncodingProvider
        Encoding big5Encoding = Encoding.GetEncoding(950);

        // 3. 将字节数组转换为字符串
        var html = big5Encoding.GetString(responseBytes);
        return html;
    }

    private List<Stock> ParseIsinHtml(string html)
    {
        var result = new List<Stock>();
        var context = BrowsingContext.New(Configuration.Default);
        var doc = context.OpenAsync(req => req.Content(html)).Result;

        var rows = doc.QuerySelectorAll("table.h4 tr")
                      .Skip(1);

        foreach (var row in rows)
        {
            var cols = row.QuerySelectorAll("td");
            if (cols.Length < 7) continue;

            var parts = cols[0].TextContent.Trim().Split('　');
            if (parts.Length != 2) continue;

            result.Add(new Stock(
                symbol: parts[0],
                name: parts[1],
                market: cols[3].TextContent.Trim()
            ));
        }

        return result;
    }

    private async Task SaveHtmlToFile(string html, string fileName)
    {
        try
        {
            // 使用 Big5 编码（Code Page 950）来写入文件，因为内容是从 Big5 编码的网页获取的
            Encoding big5Encoding = Encoding.GetEncoding(950);
            await File.WriteAllTextAsync(fileName, html, big5Encoding);
            System.Console.WriteLine($"[INFO] HTML 内容已成功保存到文件: {fileName}");
        }
        catch (System.Exception ex)
        {
            // 捕获可能的文件写入错误
            System.Console.WriteLine($"[ERROR] 保存文件失败 {fileName}: {ex.Message}");
        }
    }

    private static bool TryParseTwseInternalToDomain(List<string> data, string symbol, out DailyStockPrice? domainModel)
    {
        domainModel = null;
        if (!DateHelper.TryParseRocDate(data[0], out var dateOnly))
        {
            return false;
        }

        if (!decimal.TryParse(data[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var close))
        {
            return false;
        }

        domainModel = new DailyStockPrice(
            symbol: symbol,
            date: dateOnly,
            closePrice: new Money(close)
        );

        return true;
    }
}
