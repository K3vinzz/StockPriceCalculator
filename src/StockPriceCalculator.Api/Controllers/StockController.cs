using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockPriceCalculator.Api.Contracts.Stocks;
using StockPriceCalculator.Application.Stocks.Commands;
using StockPriceCalculator.Application.Stocks.Handlers;
using StockPriceCalculator.Domain.Stocks;
using StockPriceCalculator.Domain.Stocks.Interfaces;
using StockPriceCalculator.Infrastructure.Services;

namespace StockPriceCalculator.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IMarketDataProvider _marketDataProvider;
        private readonly StockListImportService _stockListImportService;
        private readonly CalculateSettlementHandler _calculateSettlementHandler;
        private readonly IStockInfoRepository _stockInfoRepo;
        private readonly ILogger<StockController> _logger;

        public StockController(
            IMarketDataProvider marketDataProvider,
            StockListImportService stockListImportService,
            CalculateSettlementHandler calculateSettlementHandler,
            IStockInfoRepository stockInfoRepository,
            ILogger<StockController> logger)
        {
            _marketDataProvider = marketDataProvider;
            _stockListImportService = stockListImportService;
            _calculateSettlementHandler = calculateSettlementHandler;
            _stockInfoRepo = stockInfoRepository;
            _logger = logger;
        }

        // GET api/marketdata/twse-daily
        [HttpGet("twse-daily")]
        public async Task<ActionResult<List<DailyStockPrice>>> GetTwseDaily([FromQuery] string symbol, [FromQuery] DateOnly date)
        {
            var data = await _marketDataProvider.GetTwseDaily(symbol, date);

            if (data is null || data.Count == 0)
            {
                _logger.LogInformation("No TWSE daily data.");
                return NoContent(); // 204
            }

            return Ok(data); // 200 + body
        }

        // GET api/marketdata/twse-daily
        [HttpGet("tpex-daily")]
        public async Task<ActionResult<List<DailyStockPrice>>> GetTpexDaily([FromQuery] string symbol, [FromQuery] DateOnly date)
        {
            var data = await _marketDataProvider.GetTpexDaily(symbol, date);

            if (data is null || data.Count == 0)
            {
                _logger.LogInformation("No TPEX daily data.");
                return NoContent(); // 204
            }

            return Ok(data); // 200 + body
        }


        [HttpGet("import-list")]
        public async Task<ActionResult> ImportIsinStockList()
        {
            await _stockListImportService.ImportStockListAsync();

            return Ok();
        }

        /// <summary>
        /// 計算單筆股票交割金額
        /// </summary>
        [HttpPost("CalculateSettlement")]
        public async Task<ActionResult<CalculateSettlementResponse>> CalculateSettlementAsync(
            [FromBody] CalculateSettlementRequest request)
        {
            // string → DateOnly 轉換
            if (!DateOnly.TryParse(request.Date, out var tradeDate))
            {
                return BadRequest("Invalid date format. Expected yyyy-MM-dd.");
            }

            var command = new CalculateSettlementCommand(request.Symbol, tradeDate, request.Quantity, request.Market);
            var result = await _calculateSettlementHandler.HandleAsync(command);

            return new CalculateSettlementResponse(
                Symbol: result.Symbol,
                TradeDate: result.TradeDate.ToString("yyyy-MM-dd"),
                Shares: result.Shares,
                ClosePrice: result.ClosePrice,
                TotalAmount: result.TotalAmount,
                HasPriceData: result.ClosePrice > 0 && result.TotalAmount > 0
            );
        }

        [HttpGet("search")]
        public async Task<ActionResult<StockSearchResponse>> SearchStocks([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Ok(new StockSearchResponse([]));

            var stocks = await _stockInfoRepo.SearchStocksAsync(keyword);

            var response = new StockSearchResponse(stocks);

            return Ok(response);
        }
    }
}
