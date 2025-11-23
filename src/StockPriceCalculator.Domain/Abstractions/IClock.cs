namespace StockPriceCalculator.Domain.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
