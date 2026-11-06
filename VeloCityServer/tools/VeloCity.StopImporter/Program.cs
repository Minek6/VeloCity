using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VeloCity.Api.Models.Data;
using VeloCity.Api.Models;
using System.Text.Json;


var apiDirectory = Path.GetFullPath(
    Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..", "..",
        "VeloCity.Api"));

var configuration = new ConfigurationBuilder()
    .SetBasePath(apiDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile(
        "appsettings.Development.json",
        optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration
    .GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine(
        "ERROR: DefaultConnection is not configured.");
    return;
}


var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql(connectionString)
    .Options;

await using var context = new ApplicationDbContext(options);

if (!await context.Database.CanConnectAsync())
{
    Console.WriteLine(
        "ERROR: Cannot connect to PostgreSQL.");
    return;
}

var existingStops = await context.Stops.CountAsync();


var existingOsmIds = (await context.Stops
    .AsNoTracking()
    .Where(s => s.OsmId != null && s.OsmType != null)
    .Select(s => new { s.OsmType, s.OsmId })
    .ToListAsync())
    .Select(s => (Type: s.OsmType!, Id: s.OsmId!.Value))
    .ToHashSet();

int newStops = 0;
int alreadyImported = 0;


Console.WriteLine("PostgreSQL connection successful.");
Console.WriteLine($"Existing stops: {existingStops}");


if (args.Length == 0)
{
    Console.WriteLine("Usage: VeloCity.StopImporter <geojson-path>");
    return;
}

var filePath = args[0];

if (!File.Exists(filePath))
{
    Console.WriteLine($"File not found: {filePath}");
    return;
}

using var stream = File.OpenRead(filePath);
using var document = await JsonDocument.ParseAsync(stream);


var firstFeature = document.RootElement
    .GetProperty("features")[0];

Console.WriteLine("=== FIRST GEOJSON FEATURE ===");

Console.WriteLine(
    JsonSerializer.Serialize(
        firstFeature,
        new JsonSerializerOptions
        {
            WriteIndented = true
        }));

Console.WriteLine("=============================");


var features = document.RootElement
    .GetProperty("features")
    .EnumerateArray();

int total = 0;
int valid = 0;
int skipped = 0;


var osmIdentifiers = new HashSet<(string Type, long Id)>();

int duplicates = 0;
int invalidCoordinates = 0;


var stopsToImport = new List<StopImportItem>();




foreach (var feature in features)
{
    total++;

    
if (!feature.TryGetProperty("id", out var idElement))
{
    skipped++;
    continue;
}

var osmIdentifier = idElement.GetString();

if (string.IsNullOrWhiteSpace(osmIdentifier))
{
    skipped++;
    continue;
}

var parts = osmIdentifier.Split('/');

if (parts.Length != 2 ||
    (parts[0] != "node" &&
     parts[0] != "way" &&
     parts[0] != "relation") ||
    !long.TryParse(parts[1], out var osmId) ||
    osmId <= 0)
{
    skipped++;
    continue;
}

var osmType = parts[0];


    var properties = feature.GetProperty("properties");

    if (!properties.TryGetProperty("name", out var nameElement))
    {
        skipped++;
        continue;
    }

    var name = nameElement.GetString();

    if (string.IsNullOrWhiteSpace(name))
    {
        skipped++;
        continue;
    }

    var geometry = feature.GetProperty("geometry");

    if (geometry.GetProperty("type").GetString() != "Point")
    {
        skipped++;
        continue;
    }

    var coordinates = geometry
        .GetProperty("coordinates")
        .EnumerateArray()
        .ToArray();

    var longitude = coordinates[0].GetDouble();
    var latitude = coordinates[1].GetDouble();

    
if (!double.IsFinite(latitude) ||
    !double.IsFinite(longitude) ||
    latitude < -90 ||
    latitude > 90 ||
    longitude < -180 ||
    longitude > 180 ||
    (latitude == 0 && longitude == 0))
{
    invalidCoordinates++;
    skipped++;
    continue;
}

if (!osmIdentifiers.Add((osmType, osmId)))
{
    duplicates++;
    skipped++;
    continue;
}




if (existingOsmIds.Contains((osmType, osmId)))
{
    alreadyImported++;
}
else
{
    newStops++;

    stopsToImport.Add(new StopImportItem(
        name,
        latitude,
        longitude,
        osmType,
        osmId
    ));
}



    valid++;

    
    Console.WriteLine(
        $"{name} | OSM: {osmType}/{osmId} | " +
        $"Latitude: {latitude} | Longitude: {longitude}");

}

Console.WriteLine();
Console.WriteLine($"Total features: {total}");
Console.WriteLine($"Valid stops: {valid}");
Console.WriteLine($"Skipped: {skipped}");

Console.WriteLine($"Duplicate OSM IDs: {duplicates}");
Console.WriteLine($"Invalid coordinates: {invalidCoordinates}");


Console.WriteLine();
Console.WriteLine("=== IMPORT PREVIEW ===");
Console.WriteLine($"New stops: {newStops}");
Console.WriteLine($"Already imported: {alreadyImported}");
Console.WriteLine(
    args.Contains("--apply")
        ? "Mode: APPLY"
        : "Database changes: 0 (dry-run)");

Console.WriteLine(
    $"Prepared for import: {stopsToImport.Count}");


var applyChanges = args.Contains("--apply");

if (!applyChanges)
{
    Console.WriteLine();
    Console.WriteLine("DRY-RUN: No changes were made.");
    Console.WriteLine("Use --apply to import stops.");
    return;
}

if (stopsToImport.Count == 0)
{
    Console.WriteLine("No new stops to import.");
    return;
}

// Pobieramy największy istniejący ExternalId.
var maxExternalId = await context.Stops
    .MaxAsync(s => (int?)s.ExternalId) ?? 0;

// Sprawdzamy, czy nie przekroczymy zakresu int.
if ((long)maxExternalId + stopsToImport.Count > int.MaxValue)
{
    Console.WriteLine("ERROR: ExternalId range exceeded.");
    return;
}

var nextExternalId = maxExternalId;

var stopsToSave = stopsToImport.Select(item => new Stop
{
    ExternalId = ++nextExternalId,
    Name = item.Name,
    Latitude = item.Latitude,
    Longitude = item.Longitude,
    OsmType = item.OsmType,
    OsmId = item.OsmId,
    IsActive = true,
    ZoneId = 0
}).ToList();

Console.WriteLine();
Console.WriteLine($"Importing {stopsToSave.Count} stops...");

await using var transaction =
    await context.Database.BeginTransactionAsync();

try
{
    context.Stops.AddRange(stopsToSave);

    await context.SaveChangesAsync();

    await transaction.CommitAsync();

    Console.WriteLine("Import completed successfully.");
    Console.WriteLine($"Imported stops: {stopsToSave.Count}");
}
catch (Exception ex)
{
    await transaction.RollbackAsync();

    Console.WriteLine("Import failed. Transaction rolled back.");
    Console.WriteLine(ex.Message);

    throw;
}


record StopImportItem(
    string Name,
    double Latitude,
    double Longitude,
    string OsmType,
    long OsmId
);