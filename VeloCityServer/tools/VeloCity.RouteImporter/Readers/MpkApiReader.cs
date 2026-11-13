using System.Globalization;
using VeloCity.RouteImporter.Models;
using System.Text.Json;

namespace VeloCity.RouteImporter.Readers;

public static class MpkApiReader
{
    public static async Task<MpkLineData> ReadAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"ERROR: File not found: {filePath}");
            return new MpkLineData();
        }

        using var document = JsonDocument.Parse(
            await File.ReadAllTextAsync(filePath));

        var root = document.RootElement;

        var lineName = root.GetProperty("name").GetString();
        var timetableId = root.GetProperty("timetables_id").GetInt32();

        var variants = root.GetProperty("variants")
            .EnumerateArray()
            .ToList();

        var mpkVariants = variants
            .Select(v => new MpkRouteVariant(
                v.GetProperty("id").GetInt32(),
                v.GetProperty("name").GetString() ?? "",
                v.GetProperty("direction").GetInt32(),
                v.GetProperty("runs").GetArrayLength()
            ))
            .ToList();

        Console.WriteLine("=== MPK API Reader ===");
        Console.WriteLine($"Line: {lineName}");
        Console.WriteLine($"Timetable ID: {timetableId}");
        Console.WriteLine($"Variants: {variants.Count}");

        var totalRuns = 0;

        foreach (var variant in variants)
        {
            var variantId = variant.GetProperty("id").GetInt32();
            var variantName = variant.GetProperty("name").GetString();
            var direction = variant.GetProperty("direction").GetInt32();

            var runsCount = variant.GetProperty("runs")
                .GetArrayLength();

            totalRuns += runsCount;

            Console.WriteLine(
                $"Variant {variantId}: {variantName} " +
                $"(direction: {direction}, runs: {runsCount})");
        }

        
Console.WriteLine();
Console.WriteLine("=== Sample runs ===");

foreach (var variant in variants)
{
    var runs = variant.GetProperty("runs");

    if (runs.GetArrayLength() == 0)
        continue;

    var firstRun = runs[0];

    var startMinutes = firstRun
        .GetProperty("start_time")
        .GetInt32();

    var startTime = TimeOnly.FromTimeSpan(
        TimeSpan.FromMinutes(startMinutes));

    var runId = firstRun.GetProperty("id").GetInt32();

    var stopsCount = firstRun
        .GetProperty("stops")
        .GetArrayLength();

    var monday = firstRun.GetProperty("is_monday").GetInt32();
    var saturday = firstRun.GetProperty("is_saturday").GetInt32();
    var sunday = firstRun.GetProperty("is_sunday").GetInt32();

    Console.WriteLine(
        $"Variant: {variant.GetProperty("id").GetInt32()}");
    Console.WriteLine($"Run: {runId}");
    Console.WriteLine($"Start: {startTime:HH:mm}");
    Console.WriteLine($"Stops in response: {stopsCount}");
    Console.WriteLine(
        $"Monday: {monday}, Saturday: {saturday}, Sunday: {sunday}");
    Console.WriteLine();
}


        Console.WriteLine($"Total runs: {totalRuns}");

Console.WriteLine();
Console.WriteLine("=== MPK stops ===");

var stopRecords = variants
    .SelectMany(v => v.GetProperty("runs").EnumerateArray())
    .SelectMany(r => r.GetProperty("stops").EnumerateArray())
    .Where(s => s.TryGetProperty("stop_order", out _))
    .ToList();

var uniqueStops = stopRecords
    .GroupBy(s => s.GetProperty("id").GetInt32())
    .Select(g => g.First())
    .OrderBy(s => s.GetProperty("id").GetInt32())
    .ToList();

var mpkStops = uniqueStops.Select(s => new MpkStop(
    s.GetProperty("id").GetInt32(),
    s.GetProperty("number").GetString() ?? "",
    s.GetProperty("name").GetString() ?? "",
    double.Parse(
        s.GetProperty("latitude").GetString()!,
        CultureInfo.InvariantCulture),
    double.Parse(
        s.GetProperty("longitude").GetString()!,
        CultureInfo.InvariantCulture)
)).ToList();

Console.WriteLine($"Stop occurrences: {stopRecords.Count}");
Console.WriteLine($"Unique MPK stop IDs: {uniqueStops.Count}");

Console.WriteLine();
Console.WriteLine("=== Sample MPK stops ===");

foreach (var stop in uniqueStops.Take(5))
{
    Console.WriteLine(stop.GetRawText());
}
        
Console.WriteLine();
Console.WriteLine("=== MPK stop number validation ===");

var duplicateNumbers = uniqueStops
    .GroupBy(s => s.GetProperty("number").GetString())
    .Where(g => g.Count() > 1)
    .ToList();

Console.WriteLine(
    $"Unique stop numbers: {uniqueStops.Select(s => s.GetProperty("number").GetString()).Distinct().Count()}");

Console.WriteLine(
    $"Duplicate stop numbers: {duplicateNumbers.Count}");

foreach (var group in duplicateNumbers)
{
    Console.WriteLine($"Number: {group.Key}");

    foreach (var stop in group)
    {
        Console.WriteLine(
            $"  ID: {stop.GetProperty("id").GetInt32()}, " +
            $"Name: {stop.GetProperty("name").GetString()}");
    }
}

Console.WriteLine();
Console.WriteLine("=== All unique MPK stops ===");

foreach (var stop in uniqueStops
    .OrderBy(s => s.GetProperty("number").GetString()))
{
    var id = stop.GetProperty("id").GetInt32();
    var number = stop.GetProperty("number").GetString();
    var name = stop.GetProperty("name").GetString();
    var latitude = stop.GetProperty("latitude").GetString();
    var longitude = stop.GetProperty("longitude").GetString();

    Console.WriteLine(
        $"{number} | ID: {id} | {name} | {latitude}, {longitude}");
}

Console.WriteLine();
Console.WriteLine("=== Run stop times ===");

var selectedVariant = variants.First(v =>
    v.GetProperty("id").GetInt32() == 43708);

var selectedRun = selectedVariant
    .GetProperty("runs")[0];

var departureMinutes = selectedRun
    .GetProperty("start_time").GetInt32();

var currentTime = TimeSpan.FromMinutes(departureMinutes);

var orderedStops = selectedRun
    .GetProperty("stops")
    .EnumerateArray()
    .Where(s => s.TryGetProperty("stop_order", out _))
    .OrderBy(s => s.GetProperty("stop_order").GetInt32())
    .ToList();

foreach (var stop in orderedStops)
{
    var stopName = stop.GetProperty("name").GetString();

    var travelSeconds = stop
        .GetProperty("stop_time").GetInt32();

    currentTime += TimeSpan.FromSeconds(travelSeconds);

    Console.WriteLine(
        $"{currentTime:hh\\:mm\\:ss} - {stopName}");
}


Console.WriteLine();
Console.WriteLine("=== Run duration validation ===");

int validRuns = 0;
int invalidRuns = 0;

foreach (var variant in variants)
{
    foreach (var run in variant.GetProperty("runs").EnumerateArray())
    {
        var runId = run.GetProperty("id").GetInt32();
        var expectedMinutes =
            run.GetProperty("total_time").GetInt32();

        var stops = run.GetProperty("stops")
            .EnumerateArray()
            .Where(s => s.TryGetProperty("stop_order", out _))
            .ToList();

        var calculatedSeconds = stops.Sum(s =>
            s.GetProperty("stop_time").GetInt32());

        var expectedSeconds = expectedMinutes * 60;

        if (calculatedSeconds == expectedSeconds)
        {
            validRuns++;
        }
        else
        {
            invalidRuns++;

            Console.WriteLine(
                $"MISMATCH: Run {runId}, " +
                $"expected {expectedSeconds}s, " +
                $"calculated {calculatedSeconds}s");
        }
    }
}

Console.WriteLine($"Valid runs: {validRuns}");
Console.WriteLine($"Invalid runs: {invalidRuns}");

        Console.WriteLine();
Console.WriteLine("=== MPK 29 Listopada stop usage ===");

var selectedNumbers = new HashSet<string>
{
    "00781",
    "00782",
    "00783"
};

foreach (var variant in variants)
{
    var variantId = variant.GetProperty("id").GetInt32();
    var variantName = variant.GetProperty("name").GetString();
    var direction = variant.GetProperty("direction").GetInt32();

    foreach (var run in variant.GetProperty("runs").EnumerateArray())
    {
        var matchingStops = run.GetProperty("stops")
            .EnumerateArray()
            .Where(s =>
                s.TryGetProperty("stop_order", out _) &&
                selectedNumbers.Contains(
                    s.GetProperty("number").GetString() ?? ""))
            .ToList();

        foreach (var stop in matchingStops)
        {
            Console.WriteLine(
                $"MPK {stop.GetProperty("number").GetString()} | " +
                $"Variant: {variantId} | " +
                $"Direction: {direction} | " +
                $"Run: {run.GetProperty("id").GetInt32()} | " +
                $"Order: {stop.GetProperty("stop_order").GetInt32()}");
        }
    }
}

        Console.WriteLine();
Console.WriteLine("=== Special run 172731 ===");

var specialVariant = variants.First(v =>
    v.GetProperty("id").GetInt32() == 43702);

var specialRun = specialVariant
    .GetProperty("runs")
    .EnumerateArray()
    .First(r => r.GetProperty("id").GetInt32() == 172731);

foreach (var stop in specialRun
    .GetProperty("stops")
    .EnumerateArray()
    .Where(s => s.TryGetProperty("stop_order", out _))
    .OrderBy(s => s.GetProperty("stop_order").GetInt32()))
{
    Console.WriteLine(
        $"{stop.GetProperty("stop_order").GetInt32()}. " +
        $"MPK {stop.GetProperty("number").GetString()} | " +
        $"{stop.GetProperty("name").GetString()} | " +
        $"{stop.GetProperty("latitude").GetString()}, " +
        $"{stop.GetProperty("longitude").GetString()}");
}

        Console.WriteLine();
Console.WriteLine("=== MPK 00781 / 00782 / 00783 details ===");

foreach (var number in new[] { "00781", "00782", "00783" })
{
    var stop = uniqueStops.First(s =>
        s.GetProperty("number").GetString() == number);

    Console.WriteLine();
    Console.WriteLine($"Stop number: {number}");
    Console.WriteLine($"MPK ID: {stop.GetProperty("id")}");
    Console.WriteLine($"Origin ID: {stop.GetProperty("origin_id")}");
    Console.WriteLine($"Name: {stop.GetProperty("name")}");
    Console.WriteLine($"Street: {stop.GetProperty("street")}");
    Console.WriteLine($"Latitude: {stop.GetProperty("latitude")}");
    Console.WriteLine($"Longitude: {stop.GetProperty("longitude")}");
}

    Console.WriteLine();
Console.WriteLine("=== Unresolved MPK stop details ===");

var unresolvedNumbers = new HashSet<string>
{
    "00783", "00802", "01202",
    "02042", "02051", "02062"
};

foreach (var stop in uniqueStops
    .Where(s => unresolvedNumbers.Contains(
        s.GetProperty("number").GetString() ?? "")))
{
    Console.WriteLine();
    Console.WriteLine(stop.GetRawText());
}

    Console.WriteLine();
Console.WriteLine("=== MPK route directions validation ===");

foreach (var variantId in new[] { 43708, 43709 })
{
    var variant = variants.Single(v =>
        v.GetProperty("id").GetInt32() == variantId);

    var run = variant.GetProperty("runs")[0];

    Console.WriteLine();
    Console.WriteLine(
        $"Variant {variantId} | " +
        $"Direction: {variant.GetProperty("direction").GetInt32()}");

    foreach (var stop in run.GetProperty("stops")
        .EnumerateArray()
        .Where(s => s.TryGetProperty("stop_order", out _))
        .OrderBy(s => s.GetProperty("stop_order").GetInt32()))
    {
        Console.WriteLine(
            $"{stop.GetProperty("stop_order").GetInt32()}. " +
            $"MPK {stop.GetProperty("number").GetString()} | " +
            $"{stop.GetProperty("name").GetString()}");
    }
}

        Console.WriteLine("READ-ONLY: No database changes.");

        return new MpkLineData
        {
            LineName = lineName ?? "",
            TimetableId = timetableId,
            Stops = mpkStops,
            Variants = mpkVariants
        };
    }
}
