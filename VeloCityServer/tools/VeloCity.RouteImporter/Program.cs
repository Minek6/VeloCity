using VeloCity.RouteImporter.Readers;
using System.Text.Json;
using VeloCity.RouteImporter.Matching;
using VeloCity.RouteImporter.Database;

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

Console.WriteLine("=== Stop matching test ===");

double distance = StopMatcher.CalculateDistanceMeters(
    49.6035, 20.7004,       // MPK 00761
    49.6035479, 20.7003905 // OSM / VeloCity ID 178
);

Console.WriteLine("=== VeloCity database stops ===");

var veloCityStops = await StopRepository.ReadStopsAsync();

await StopRepository.CheckMpkAssignmentsAsync();

Console.WriteLine();

Console.WriteLine(
    $"MPK 00761 -> VeloCity 178: {distance:F2} m");

if (args.Length > 0 &&
    args[0].EndsWith(
        "line6.json",
        StringComparison.OrdinalIgnoreCase))
{
    var mpkStops = await MpkApiReader.ReadAsync(
        Path.GetFullPath(args[0]));

    Console.WriteLine();
    Console.WriteLine("=== MPK stops returned ===");
    Console.WriteLine($"Count: {mpkStops.Count}");

    using var matchesDocument = JsonDocument.Parse(
    await File.ReadAllTextAsync(
        Path.Combine("data", "approved_stop_matches.json")));

var approvedMatches =
    matchesDocument.RootElement.Deserialize<
        VeloCity.RouteImporter.Models.ApprovedStopMatches>()
    ?? throw new InvalidOperationException(
        "Nie można odczytać powiązań MPK.");

Console.WriteLine();
Console.WriteLine("=== Approved stop matches validation ===");

var errors = new List<string>();

if (approvedMatches.Line != "6")
    errors.Add("Nieprawidłowy numer linii.");

if (approvedMatches.TimetableId != 86)
    errors.Add("Nieprawidłowy identyfikator rozkładu.");

var mpkNumbers = mpkStops
    .Select(s => s.Number)
    .ToHashSet();

var veloCityIds = veloCityStops
    .Select(s => s.Id)
    .ToHashSet();

foreach (var match in approvedMatches.Matches)
{
    if (!mpkNumbers.Contains(match.MpkNumber))
        errors.Add($"Nieznany numer MPK: {match.MpkNumber}");

    if (!veloCityIds.Contains(match.VeloCityStopId))
        errors.Add($"Nieznany ID VeloCity: {match.VeloCityStopId}");
}

foreach (var group in approvedMatches.Matches
    .GroupBy(m => m.MpkNumber)
    .Where(g => g.Count() > 1))
{
    errors.Add($"Powtórzony numer MPK: {group.Key}");
}

foreach (var group in approvedMatches.Matches
    .GroupBy(m => m.VeloCityStopId)
    .Where(g => g.Count() > 1))
{
    errors.Add($"Powtórzony ID VeloCity: {group.Key}");
}

Console.WriteLine($"Matches: {approvedMatches.Matches.Count}");
Console.WriteLine($"Errors: {errors.Count}");

foreach (var error in errors)
{
    Console.WriteLine($"ERROR: {error}");
}

Console.WriteLine(
    errors.Count == 0
        ? "VALIDATION OK"
        : "VALIDATION FAILED");

        Console.WriteLine();
Console.WriteLine("=== Approved matches geographic validation ===");

int geographicErrors = 0;
int geographicWarnings = 0;

foreach (var match in approvedMatches.Matches)
{
    var mpk = mpkStops.Single(s =>
        s.Number == match.MpkNumber);

    var veloCity = veloCityStops.Single(s =>
        s.Id == match.VeloCityStopId);

    var distanceMeters = StopMatcher.CalculateDistanceMeters(
        mpk.Latitude,
        mpk.Longitude,
        veloCity.Latitude,
        veloCity.Longitude);

    var status = StopMatcher.ClassifyMatch(
        mpk.Name,
        veloCity.Name,
        distanceMeters);

    if (distanceMeters > 50)
    {
        geographicErrors++;

        Console.WriteLine(
            $"ERROR: MPK {mpk.Number} -> " +
            $"VeloCity {veloCity.Id} | " +
            $"{distanceMeters:F1} m");
    }
    else if (status != "PEWNE")
    {
        geographicWarnings++;

        Console.WriteLine(
            $"WARNING: MPK {mpk.Number} -> " +
            $"VeloCity {veloCity.Id} | " +
            $"{distanceMeters:F1} m | " +
            $"{mpk.Name} <> {veloCity.Name}");
    }
}

Console.WriteLine();
Console.WriteLine($"Geographic errors: {geographicErrors}");
Console.WriteLine($"Geographic warnings: {geographicWarnings}");

if (args.Contains("--commit"))
{
    if (errors.Count == 0 &&
        geographicErrors == 0 &&
        geographicWarnings == 0)
    {
        Console.WriteLine();
        Console.WriteLine("=== MPK assignments IMPORT ===");

        await StopRepository.ImportMpkAssignmentsAsync(
            approvedMatches.Matches,
            mpkStops,
            dryRun: false);
    }
    else
    {
        Console.WriteLine(
            "IMPORT BLOCKED: Validation errors or warnings.");
    }
}

    Console.WriteLine();
Console.WriteLine("=== MPK -> VeloCity matching report ===");

foreach (var mpkStop in mpkStops.OrderBy(s => s.Number))
{
    var candidates = veloCityStops
        .Where(s => s.Id > 3) // Pomijamy 3 przystanki testowe
        .Select(s => new
        {
            Stop = s,
            Distance = StopMatcher.CalculateDistanceMeters(
                mpkStop.Latitude,
                mpkStop.Longitude,
                s.Latitude,
                s.Longitude)
        })
        .OrderBy(s => s.Distance)
        .Take(3)
        .ToList();

    Console.WriteLine();
    Console.WriteLine(
        $"MPK {mpkStop.Number} | {mpkStop.Name}");

    foreach (var candidate in candidates)
{
    var status = StopMatcher.ClassifyMatch(
        mpkStop.Name,
        candidate.Stop.Name,
        candidate.Distance);

    Console.WriteLine(
        $"  VeloCity ID: {candidate.Stop.Id} | " +
        $"{candidate.Stop.Name} | " +
        $"{candidate.Distance:F1} m | " +
        $"Status: {status}");
}
}

Console.WriteLine();
Console.WriteLine();
Console.WriteLine("=== Best MPK stop matches ===");

foreach (var mpk in mpkStops.OrderBy(s => s.Number))
{
    var candidates = veloCityStops
        .Where(s => s.Id > 3)
        .Select(s => new
        {
            Stop = s,
            Distance = StopMatcher.CalculateDistanceMeters(
                mpk.Latitude,
                mpk.Longitude,
                s.Latitude,
                s.Longitude)
        })
        .Select(x => new
        {
            x.Stop,
            x.Distance,
            Status = StopMatcher.ClassifyMatch(
                mpk.Name,
                x.Stop.Name,
                x.Distance)
        })
        .Where(x => x.Status != "NIEDOPASOWANE")
        .OrderByDescending(x =>
            string.Equals(
                mpk.Name.Trim(),
                x.Stop.Name.Trim(),
                StringComparison.OrdinalIgnoreCase))
        .ThenBy(x => x.Status == "PEWNE" ? 0 : 1)
        .ThenBy(x => x.Distance)
        .ToList();

    if (candidates.Count == 0)
    {
        Console.WriteLine(
            $"MPK {mpk.Number} -> BRAK DOPASOWANIA");
        continue;
    }

    var best = candidates[0];

    var ambiguous = candidates
        .Skip(1)
        .Any(x => x.Status == best.Status &&
                  Math.Abs(x.Distance - best.Distance) <= 10);

    var result = ambiguous
        ? "NIEJEDNOZNACZNE"
        : best.Status;

    Console.WriteLine(
        $"MPK {mpk.Number} -> " +
        $"VeloCity {best.Stop.Id} | " +
        $"{best.Stop.Name} | " +
        $"{best.Distance:F1} m | " +
        $"{result}");
}
Console.WriteLine("=== Stop matching conflicts ===");

var nearestMatches = mpkStops
    .Select(mpk => new
    {
        Mpk = mpk,
        Candidate = veloCityStops
            .Where(s => s.Id > 3)
            .Select(s => new
            {
                Stop = s,
                Distance = StopMatcher.CalculateDistanceMeters(
                    mpk.Latitude,
                    mpk.Longitude,
                    s.Latitude,
                    s.Longitude)
            })
            .Select(x => new
            {
                x.Stop,
                x.Distance,
                Status = StopMatcher.ClassifyMatch(
                    mpk.Name,
                    x.Stop.Name,
                    x.Distance)
            })
            .Where(x => x.Status != "NIEDOPASOWANE")
            .OrderByDescending(x =>
                string.Equals(
                    mpk.Name.Trim(),
                    x.Stop.Name.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            .ThenBy(x => x.Status == "PEWNE" ? 0 : 1)
            .ThenBy(x => x.Distance)
            .FirstOrDefault()
    })
    .Where(x => x.Candidate != null)
    .ToList();

var conflicts = nearestMatches
    .GroupBy(x => x.Candidate.Stop.Id)
    .Where(g => g.Count() > 1)
    .ToList();

foreach (var conflict in conflicts)
{
    Console.WriteLine();
    Console.WriteLine(
        $"CONFLICT: VeloCity ID {conflict.Key}");

    foreach (var match in conflict)
    {
        Console.WriteLine(
            $"  MPK {match.Mpk.Number} | " +
            $"{match.Mpk.Name} | " +
            $"{match.Candidate.Distance:F1} m");
    }
}

Console.WriteLine();
Console.WriteLine($"Conflicting VeloCity stops: {conflicts.Count}");

    Console.WriteLine();
Console.WriteLine("=== Safe matching summary ===");

var conflictedIds = conflicts
    .Select(c => c.Key)
    .ToHashSet();

int safeCount = 0;
int reviewCount = 0;
int unmatchedCount = 0;

foreach (var match in nearestMatches.OrderBy(m => m.Mpk.Number))
{
    var candidate = match.Candidate!;

    var status = StopMatcher.ClassifyMatch(
        match.Mpk.Name,
        candidate.Stop.Name,
        candidate.Distance);

    var alternativeCandidates = veloCityStops
    .Where(s => s.Id > 3 && s.Id != candidate.Stop.Id)
    .Select(s => new
    {
        Stop = s,
        Distance = StopMatcher.CalculateDistanceMeters(
            match.Mpk.Latitude,
            match.Mpk.Longitude,
            s.Latitude,
            s.Longitude)
    })
    .Where(x =>
        StopMatcher.ClassifyMatch(
            match.Mpk.Name,
            x.Stop.Name,
            x.Distance) == status &&
        Math.Abs(x.Distance - candidate.Distance) <= 10)
    .ToList();

bool ambiguous = alternativeCandidates.Count > 0;

    if (status == "PEWNE" &&
    !ambiguous &&
    !conflictedIds.Contains(candidate.Stop.Id))
    {
        safeCount++;
        Console.WriteLine(
            $"READY: MPK {match.Mpk.Number} -> " +
            $"VeloCity {candidate.Stop.Id}");
    }
    else
    {
        reviewCount++;
        Console.WriteLine(
            $"REVIEW: MPK {match.Mpk.Number} -> " +
            $"VeloCity {candidate.Stop.Id}");
    }
}

unmatchedCount = mpkStops.Count - nearestMatches.Count;

Console.WriteLine();
Console.WriteLine($"Ready: {safeCount}");
Console.WriteLine($"Review: {reviewCount}");
Console.WriteLine($"Unmatched: {unmatchedCount}");

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
