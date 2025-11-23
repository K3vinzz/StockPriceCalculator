using System;
using StockPriceCalculator.Domain.Stocks;
using StockPriceCalculator.Domain.Stocks.Interfaces;

namespace StockPriceCalculator.Infrastructure.Repositories;

public class StockValuationRecordRepository : IStockValuationRecordRepository
{
    public Task AddAsync(StockValuationRecord record, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
