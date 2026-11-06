
using System.Text.Json;

namespace VeloCity.RouteImporter.Readers;

public static class MpkApiReader
{
    public static async Task ReadAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"ERROR: File not found: {filePath}");
            return;
        }

        using var document = JsonDocument.Parse(
            await File.ReadAllTextAsync(filePath));

        var root = document.RootElement;

        var lineName = root.GetProperty("name").GetString();
        var timetableId = root.GetProperty("timetables_id").GetInt32();

        var variants = root.GetProperty("variants")
            .EnumerateArray()
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


        Console.WriteLine("READ-ONLY: No database changes.");
    }
}
