
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VeloCity.Api.Models.Data;
using VeloCity.Api.Models;

// Lista linii MPK Nowy Sącz.
// Przed właściwym importem zweryfikujemy ją
// z aktualnym wykazem przewoźnika.

var cityLines = new[]
{
    "3", "4", "6", "7", "9", "12", "13",
    "23", "28", "31", "35", "36", "41",
    "42", "44", "51", "52"
};

var suburbanLines = new[]
{
    "1", "2", "5", "10", "11", "14",
    "15", "17", "18", "19", "20", "21",
    "22", "24", "25", "26", "27", "30",
    "33", "34", "37", "39", "40", "43",
    "45", "46", "47", "48", "49"
};

var nightLines = new[]
{
    "101"
};

var allLines = cityLines
    .Concat(suburbanLines)
    .Concat(nightLines)
    .Distinct()
    .OrderBy(name => int.Parse(name))
    .ToList();

Console.WriteLine("=== VeloCity Line Importer ===");
Console.WriteLine();

Console.WriteLine($"City lines: {cityLines.Length}");
Console.WriteLine($"Suburban lines: {suburbanLines.Length}");
Console.WriteLine($"Night lines: {nightLines.Length}");
Console.WriteLine($"Total unique lines: {allLines.Count}");

Console.WriteLine();
Console.WriteLine("Lines prepared for import:");

foreach (var line in allLines)
{
    Console.WriteLine($"  {line}");
}


var apiDirectory = Path.GetFullPath(
    Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..", "..",
        "VeloCity.Api"
    )
);

var configuration = new ConfigurationBuilder()
    .SetBasePath(apiDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration
    .GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("ERROR: Missing database connection string.");
    return;
}

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql(connectionString)
    .Options;

await using var context = new ApplicationDbContext(options);

if (!await context.Database.CanConnectAsync())
{
    Console.WriteLine("ERROR: Cannot connect to PostgreSQL.");
    return;
}

Console.WriteLine();
Console.WriteLine("PostgreSQL connection successful.");

var existingLines = await context.Lines
    .AsNoTracking()
    .Select(line => line.Name)
    .ToListAsync();

Console.WriteLine($"Existing lines: {existingLines.Count}");

var existingNames = existingLines
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

var newLines = allLines
    .Where(name => !existingNames.Contains(name))
    .ToList();

Console.WriteLine($"New lines prepared: {newLines.Count}");
Console.WriteLine($"Already existing: {allLines.Count - newLines.Count}");


Console.WriteLine();

var applyChanges = args.Contains("--apply");

if (!applyChanges)
{
    Console.WriteLine();
    Console.WriteLine("DRY-RUN: No database changes.");
    return;
}

if (newLines.Count == 0)
{
    Console.WriteLine("No new lines to import.");
    return;
}

var linesToSave = newLines
    .Select(name => new Line
    {
        Name = name,
        IsActive = true
    })
    .ToList();

await using var transaction =
    await context.Database.BeginTransactionAsync();

try
{
    context.Lines.AddRange(linesToSave);

    await context.SaveChangesAsync();

    await transaction.CommitAsync();

    Console.WriteLine();
    Console.WriteLine($"Imported lines: {linesToSave.Count}");
    Console.WriteLine("Import completed successfully.");
}
catch (Exception ex)
{
    await transaction.RollbackAsync();

    Console.WriteLine($"Import failed: {ex.Message}");

    throw;
}

