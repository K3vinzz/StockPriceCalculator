using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StockPriceCalculator.Api.Contracts.Stocks;

public sealed record class CalculateSettlementRequest
{
    [Required]
    [JsonPropertyName("symbol")]
    public string Symbol { get; init; } = string.Empty;

    [Required]
    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    [Required]
    [JsonPropertyName("quantity")]
    public int Quantity { get; init; }

    [Required]
    [JsonPropertyName("market")]
    public string Market { get; init; } = string.Empty;
}
