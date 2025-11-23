using Microsoft.Extensions.Logging;
using NSubstitute;
using StockPriceCalculator.Domain.Stocks;
using StockPriceCalculator.Infrastructure.Services;
using StockPriceCalculator.Infrastructure.Services.Dtos;

namespace StockPriceCalculator.Intrastructure.UnitTests.Services;

public class MarketDataProviderTests
{
    private IHttpClientFactory _httpClientFactory;
    private ILogger<MarketDataProvider> _logger;

    public MarketDataProviderTests()
    {
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _logger = Substitute.For<ILogger<MarketDataProvider>>();
    }

    [Fact]
    public async Task GetTwseDaily_RetureMappedResult()
    {
        // Given
        var symbol = "2330";
        var date = new DateOnly(2025, 11, 20);

        var fakeResponse = new TwseStockDayResponse
        {
            Data =
            [
                [
                    "114/11/19",
                    "42,661,060",
                    "59,869,558,425",
                    "1,400.00",
                    "1,415.00",
                    "1,395.00",
                    "1,395.00",
                    "-10.00",
                    "109,706"
                ],
                [
                    "114/11/20",
                    "28,295,025",
                    "41,079,763,730",
                    "1,450.00",
                    "1,460.00",
                    "1,440.00",
                    "1,455.00",
                    "+60.00",
                    "54,456"
                ],
                [
                    "114/11/21",
                    "64,673,494",
                    "90,158,936,045",
                    "1,395.00",
                    "1,405.00",
                    "1,385.00",
                    "1,385.00",
                    "-70.00",
                    "347,100"
                ]
            ]
        };

        var httpMessageHandler = new FakeHttpMessageHandler(fakeResponse);
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri("https://www.twse.com.tw/")
        };

        _httpClientFactory.CreateClient("TWSE_internal").Returns(httpClient);

        var sut = new MarketDataProvider(_httpClientFactory, _logger);

        // When
        var result = await sut.GetTwseDaily(symbol, date);

        // Then
        // 1. 檢查 HttpClient 有被呼叫、URL 正確
        Assert.NotNull(httpMessageHandler.LastRequest);
        var uri = httpMessageHandler.LastRequest!.RequestUri!;
        Assert.Equal("/exchangeReport/STOCK_DAY", uri.AbsolutePath); // 依你 TwseDailyInternalEndpoint 實際路徑調整

        var query = uri.Query.TrimStart('?');
        Assert.Contains("date=20251120", query);
        Assert.Contains("stockNo=2330", query);

        // 2. 檢查回傳資料筆數、內容
        Assert.Equal(3, result.Count);

        var first = result[0];
        Assert.Equal(new StockSymbol("2330"), first.Symbol);
        Assert.Equal(new DateOnly(2025, 11, 19), first.Date);
        Assert.Equal(1395m, first.ClosePrice.Amount);

        var second = result[1];
        Assert.Equal(new StockSymbol("2330"), second.Symbol);
        Assert.Equal(new DateOnly(2025, 11, 20), second.Date);
        Assert.Equal(1455m, second.ClosePrice.Amount);

        var third = result[2];
        Assert.Equal(new StockSymbol("2330"), third.Symbol);
        Assert.Equal(new DateOnly(2025, 11, 21), third.Date);
        Assert.Equal(1385m, third.ClosePrice.Amount);

        // 如果你有 Name、Market mapping，也可以順便驗證：
        // Assert.Equal("台積電", second.Name);
        // Assert.Equal("TWSE", second.Market);
    }
}