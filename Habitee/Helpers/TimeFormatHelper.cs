using System.Globalization;

namespace Habitee.Helpers;

public static class TimeFormatHelper
{
    private static readonly string[] AcceptedFormats =
    [
        "h:mm tt",
        "hh:mm tt",
        "h:mmtt",
        "hh:mmtt",
        "h tt",
        "hh tt",
        "htt",
        "h:mm",
        "hh:mm",
        "H:mm",
        "HH:mm"
    ];

    public static string ToDisplayTime(string? value)
    {
        return TryParse(value, out var parsedTime)
            ? parsedTime.ToString("h:mm tt", CultureInfo.InvariantCulture)
            : string.Empty;
    }

    public static bool TryNormalizeTo24Hour(string? value, out string normalizedTime)
    {
        if (TryParse(value, out var parsedTime))
        {
            normalizedTime = parsedTime.ToString("HH:mm", CultureInfo.InvariantCulture);
            return true;
        }

        normalizedTime = string.Empty;
        return false;
    }

    private static bool TryParse(string? value, out TimeOnly parsedTime)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            parsedTime = default;
            return false;
        }

        return TimeOnly.TryParseExact(value.Trim(), AcceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime)
            || TimeOnly.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedTime)
            || TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime);
    }
}
