using System;

namespace StockPriceCalculator.Infrastructure.Services.Dtos;

public sealed class TpexOuterResponse
{
    public string Stat { get; init; } = default!;   // OK, 參數輸入錯誤, ...
    public List<TpexTableItem>? Tables { get; init; }
}

public sealed class TpexTableItem
{
    public List<List<string>> Data { get; init; } = [];
}

