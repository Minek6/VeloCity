
using System.Text.RegularExpressions;
using VeloCity.RouteImporter.Models;

namespace VeloCity.RouteImporter.Readers;

public static class MpkDepartureParser
{
    public static List<MpkDeparture> Parse(
        IEnumerable<string> lines)
    {
        var departures = new List<MpkDeparture>();

        foreach (var line in lines)
        {
            // Przykłady:
            // 8 51
            // 11 1, 56
            // 11 1D, 56F

            var match = Regex.Match(
                line.Trim(),
                @"^(\d{1,2})\s+(.+)$");

            if (!match.Success)
                continue;

            if (!int.TryParse(
                    match.Groups[1].Value,
                    out var hour) ||
                hour is < 0 or > 23)
            {
                continue;
            }

            var minuteEntries = match.Groups[2].Value
                .Split(',', StringSplitOptions.TrimEntries);

            foreach (var entry in minuteEntries)
            {
                var minuteMatch = Regex.Match(
                    entry,
                    @"^(\d{1,2})([A-Za-z]*)$");

                if (!minuteMatch.Success)
                    continue;

                var minute = int.Parse(
                    minuteMatch.Groups[1].Value);

                if (minute is < 0 or > 59)
                    continue;

                var symbol = minuteMatch.Groups[2].Value;

                departures.Add(new MpkDeparture
                {
                    Time = new TimeOnly(hour, minute),
                    CourseSymbol = string.IsNullOrEmpty(symbol)
                        ? null
                        : symbol.ToUpperInvariant()
                });
            }
        }

        return departures;
    }
}
