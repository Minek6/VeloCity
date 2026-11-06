using VeloCity.RouteImporter.Readers;
using System.Text.Json;

Console.WriteLine("=== VeloCity Route Importer ===");

if (args.Length == 0)
{
    Console.WriteLine("ERROR: Missing JSON file path.");
    return;
}

if (args.Length > 0 &&
    args[0].EndsWith(
        ".pdf",
        StringComparison.OrdinalIgnoreCase))
{
    MpkPdfReader.Read(Path.GetFullPath(args[0]));
    return;
}


if (args.Length > 0 &&
    args[0].EndsWith(
        "line6.json",
        StringComparison.OrdinalIgnoreCase))
{
    await MpkApiReader.ReadAsync(Path.GetFullPath(args[0]));
    return;
}


if (args.Contains("--test-departures"))
{
    Console.WriteLine("=== Departure parser test ===");

    var testLines = new[]
    {
        "8 51",
        "11 1D, 56F",
        "15 3DF",
        "19 9"
    };

    var departures = MpkDepartureParser.Parse(testLines);

    foreach (var departure in departures)
    {
        Console.WriteLine(
            $"{departure.Time:HH:mm} " +
            $"Symbol: {departure.CourseSymbol ?? "-"}");
    }

    Console.WriteLine($"Total: {departures.Count}");
    return;
}


var filePath = Path.GetFullPath(args[0]);

if (!File.Exists(filePath))
{
    Console.WriteLine($"ERROR: File not found: {filePath}");
    return;
}

using var document = JsonDocument.Parse(
    await File.ReadAllTextAsync(filePath)
);

var elements = document.RootElement
    .GetProperty("elements")
    .EnumerateArray()
    .ToList();

const long routeOsmId = 15079739;

var route = elements.FirstOrDefault(element =>
    element.GetProperty("type").GetString() == "relation" &&
    element.GetProperty("id").GetInt64() == routeOsmId
);

if (route.ValueKind == JsonValueKind.Undefined)
{
    Console.WriteLine("ERROR: Route relation not found.");
    return;
}

var tags = route.GetProperty("tags");

var lineNumber = tags.GetProperty("ref").GetString();
var routeName = tags.GetProperty("name").GetString();

Console.WriteLine($"Line: {lineNumber}");
Console.WriteLine($"Route: {routeName}");

var platformIds = elements
    .Where(element =>
        element.GetProperty("type").GetString() == "node" &&
        element.TryGetProperty("tags", out var nodeTags) &&
        nodeTags.TryGetProperty("public_transport", out var pt) &&
        pt.GetString() == "platform"
    )
    .Select(element => element.GetProperty("id").GetInt64())
    .ToHashSet();

var platformSequence = route
    .GetProperty("members")
    .EnumerateArray()
    .Where(member =>
        member.GetProperty("type").GetString() == "node" &&
        platformIds.Contains(
            member.GetProperty("ref").GetInt64()
        )
    )
    .Select(member => member.GetProperty("ref").GetInt64())
    .ToList();

Console.WriteLine();
Console.WriteLine($"Platforms in route: {platformSequence.Count}");

var platformNames = elements
    .Where(element =>
        element.GetProperty("type").GetString() == "node" &&
        platformIds.Contains(
            element.GetProperty("id").GetInt64()
        )
    )
    .ToDictionary(
        element => element.GetProperty("id").GetInt64(),
        element =>
        {
            if (element.GetProperty("tags")
                .TryGetProperty("name", out var name))
            {
                return name.GetString() ?? "(unnamed)";
            }

            return "(unnamed)";
        }
    );

Console.WriteLine();
Console.WriteLine("Stop sequence:");

for (var index = 0; index < platformSequence.Count; index++)
{
    var osmId = platformSequence[index];

    Console.WriteLine(
        $"{index + 1}. {platformNames[osmId]} (OSM: {osmId})"
    );
}


Console.WriteLine();
Console.WriteLine("=== Route validation ===");

var expectedStart = "Sucharskiego - Westerplatte";
var expectedEnd = "29 listopada";

var firstStop = platformNames[platformSequence.First()];
var lastStop = platformNames[platformSequence.Last()];

var startValid = string.Equals(
    firstStop,
    expectedStart,
    StringComparison.OrdinalIgnoreCase
);

var endValid = string.Equals(
    lastStop,
    expectedEnd,
    StringComparison.OrdinalIgnoreCase
);

Console.WriteLine($"Start stop: {firstStop}");
Console.WriteLine($"Expected start: {expectedStart}");
Console.WriteLine($"Start valid: {startValid}");

Console.WriteLine();

Console.WriteLine($"End stop: {lastStop}");
Console.WriteLine($"Expected end: {expectedEnd}");
Console.WriteLine($"End valid: {endValid}");

if (!startValid || !endValid)
{
    Console.WriteLine();
    Console.WriteLine("WARNING: Route requires manual verification.");
    Console.WriteLine("Import blocked.");
}
else
{
    Console.WriteLine();
    Console.WriteLine("Route endpoints verified.");
}


Console.WriteLine();
Console.WriteLine("=== Official MPK comparison ===");

var officialFile = Path.Combine(
    "tools",
    "VeloCity.RouteImporter",
    "data",
    "line6_official_stops.json"
);

if (!File.Exists(officialFile))
{
    Console.WriteLine("ERROR: Official stops file not found.");
    return;
}

using var officialDocument = JsonDocument.Parse(
    await File.ReadAllTextAsync(officialFile)
);

var officialStops = officialDocument.RootElement
    .GetProperty("stopNames")
    .EnumerateArray()
    .Select(item => item.GetString() ?? string.Empty)
    .ToList();

static string NormalizeStopName(string name)
{
    return string.Concat(
        name.ToLowerInvariant()
            .Normalize(System.Text.NormalizationForm.FormD)
            .Where(c =>
                System.Globalization.CharUnicodeInfo
                    .GetUnicodeCategory(c) !=
                System.Globalization.UnicodeCategory.NonSpacingMark)
            .Where(char.IsLetterOrDigit)
    );
}


static int LevenshteinDistance(string first, string second)
{
    var a = NormalizeStopName(first);
    var b = NormalizeStopName(second);

    var distances = new int[a.Length + 1, b.Length + 1];

    for (var i = 0; i <= a.Length; i++)
        distances[i, 0] = i;

    for (var j = 0; j <= b.Length; j++)
        distances[0, j] = j;

    for (var i = 1; i <= a.Length; i++)
    {
        for (var j = 1; j <= b.Length; j++)
        {
            var cost = a[i - 1] == b[j - 1] ? 0 : 1;

            distances[i, j] = Math.Min(
                Math.Min(
                    distances[i - 1, j] + 1,
                    distances[i, j - 1] + 1
                ),
                distances[i - 1, j - 1] + cost
            );
        }
    }

    return distances[a.Length, b.Length];
}


var osmNames = platformSequence
    .Select(id => platformNames[id])
    .ToList();

var normalizedOsmNames = osmNames
    .Select(NormalizeStopName)
    .ToHashSet();

var normalizedOfficialNames = officialStops
    .Select(NormalizeStopName)
    .ToHashSet();

Console.WriteLine($"Official stops: {officialStops.Count}");
Console.WriteLine($"OSM platforms: {osmNames.Count}");

Console.WriteLine();
Console.WriteLine("Stops missing from OSM:");

foreach (var stopName in officialStops)
{
    if (!normalizedOsmNames.Contains(
            NormalizeStopName(stopName)))
    {
        Console.WriteLine($"  MISSING: {stopName}");
    }
}

Console.WriteLine();
Console.WriteLine("OSM stops not found in official list:");

foreach (var stopName in osmNames)
{
    if (!normalizedOfficialNames.Contains(
            NormalizeStopName(stopName)))
    {
        Console.WriteLine($"  EXTRA: {stopName}");
    }
}

Console.WriteLine();
Console.WriteLine("Comparison completed.");


Console.WriteLine();
Console.WriteLine("=== Possible stop name matches ===");

foreach (var officialName in officialStops)
{
    if (normalizedOsmNames.Contains(
        NormalizeStopName(officialName)))
    {
        continue;
    }

    var candidates = osmNames
        .Select(osmName => new
        {
            Name = osmName,
            Distance = LevenshteinDistance(
                officialName, osmName)
        })
        .Where(item => item.Distance <= 3)
        .OrderBy(item => item.Distance)
        .ToList();

    foreach (var candidate in candidates)
    {
        Console.WriteLine(
            $"POSSIBLE MATCH: {officialName} " +
            $"<=> {candidate.Name} " +
            $"(distance: {candidate.Distance})"
        );
    }
}


Console.WriteLine();
Console.WriteLine("DRY-RUN: No database changes.");
