namespace StockPriceCalculator.Infrastructure.Helpers;

public static class DateHelper
{
    public static bool TryParseRocDate(string rocDate, out DateOnly date)
    {
        date = default;
        if (String.IsNullOrEmpty(rocDate))
        {
            return false;
        }

        var normalizedRocDate = rocDate.Trim().Replace("/", "");

        if (normalizedRocDate.Length != 7)
        {
            return false;
        }

        if (!Int32.TryParse(normalizedRocDate.AsSpan(0, 3), out int rocYear))
        {
            return false;
        }

        if (!Int32.TryParse(normalizedRocDate.AsSpan(3, 2), out int month))
        {
            return false;
        }

        if (!Int32.TryParse(normalizedRocDate.AsSpan(5, 2), out int day))
        {
            return false;
        }

        int year = 1911 + rocYear;

        date = new DateOnly(year, month, day);

        return true;
    }
}
