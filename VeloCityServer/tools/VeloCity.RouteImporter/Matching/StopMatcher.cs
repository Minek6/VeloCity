namespace VeloCity.RouteImporter.Matching;

public static class StopMatcher
{
    public static double CalculateDistanceMeters(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        const double earthRadius = 6371000;

        double lat1 = latitude1 * Math.PI / 180;
        double lat2 = latitude2 * Math.PI / 180;

        double deltaLat =
            (latitude2 - latitude1) * Math.PI / 180;

        double deltaLon =
            (longitude2 - longitude1) * Math.PI / 180;

        double a =
            Math.Pow(Math.Sin(deltaLat / 2), 2) +
            Math.Cos(lat1) * Math.Cos(lat2) *
            Math.Pow(Math.Sin(deltaLon / 2), 2);

        double c = 2 * Math.Atan2(
            Math.Sqrt(a),
            Math.Sqrt(1 - a));

        return earthRadius * c;
    }

    public static string ClassifyMatch(
        string mpkName,
        string veloCityName,
        double distanceMeters)
    {
        static string Normalize(string name)
{
    var normalized = name
        .ToLowerInvariant()
        .Replace("ł", "l")
        .Normalize(System.Text.NormalizationForm.FormD);

    return string.Concat(
        normalized
            .Where(c =>
                System.Globalization.CharUnicodeInfo
                    .GetUnicodeCategory(c) !=
                System.Globalization.UnicodeCategory.NonSpacingMark)
            .Where(char.IsLetterOrDigit)
    );
}

        bool sameName =
            Normalize(mpkName) == Normalize(veloCityName);

        if (sameName && distanceMeters <= 15)
            return "PEWNE";

        if (distanceMeters <= 50)
            return "DO WERYFIKACJI";

        return "NIEDOPASOWANE";
    }
}