using System;
using StockPriceCalculator.Domain.Abstractions;

namespace StockPriceCalculator.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
