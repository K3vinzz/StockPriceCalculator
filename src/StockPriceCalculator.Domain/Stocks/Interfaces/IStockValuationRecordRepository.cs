using System;

namespace StockPriceCalculator.Domain.Stocks.Interfaces;

public interface IStockValuationRecordRepository
{
    Task AddAsync(StockValuationRecord record, CancellationToken ct = default);
}
