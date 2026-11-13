using Npgsql;
using VeloCity.RouteImporter.Models;

namespace VeloCity.RouteImporter.Database;

public static class StopRepository
{
    public static async Task<List<VeloCityStop>> ReadStopsAsync()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("VELOCITY_DB_CONNECTION")
            ?? throw new InvalidOperationException(
                "Brak zmiennej VELOCITY_DB_CONNECTION.");

        await using var connection =
            new NpgsqlConnection(connectionString);

        await connection.OpenAsync();

        const string sql = """
            SELECT "Id", "Name", "Latitude", "Longitude"
            FROM "Stops"
            ORDER BY "Id";
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        int count = 0;

        var stops = new List<VeloCityStop>();

        while (await reader.ReadAsync())
        {
            stops.Add(new VeloCityStop(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDouble(2),
                reader.GetDouble(3)
            ));
            
            count++;

            if (count <= 5)
            {
                Console.WriteLine(
                    $"{reader.GetInt32(0)} | " +
                    $"{reader.GetString(1)} | " +
                    $"{reader.GetDouble(2)}, " +
                    $"{reader.GetDouble(3)}");
            }
        }

        Console.WriteLine($"Loaded VeloCity stops: {count}");
        return stops;
    }

    public static async Task CheckMpkAssignmentsAsync()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("VELOCITY_DB_CONNECTION")
            ?? throw new InvalidOperationException(
                "Brak zmiennej VELOCITY_DB_CONNECTION.");

        await using var connection =
            new NpgsqlConnection(connectionString);

        await connection.OpenAsync();

        const string sql = """
            SELECT
                "Id",
                "ExternalMpkStopId",
                "ExternalMpkStopNumber"
            FROM "Stops"
            WHERE "ExternalMpkStopId" IS NOT NULL
            OR "ExternalMpkStopNumber" IS NOT NULL;
            """;

        await using var command =
            new NpgsqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        Console.WriteLine();
        Console.WriteLine("=== Existing MPK assignments ===");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            var id = reader.GetInt32(0);

            var mpkId = reader.IsDBNull(1)
                ? "NULL"
                : reader.GetInt32(1).ToString();

            var mpkNumber = reader.IsDBNull(2)
                ? "NULL"
                : reader.GetString(2);

            Console.WriteLine(
                $"VeloCity {id} | MPK ID: {mpkId} | Number: {mpkNumber}");
        }

        Console.WriteLine($"Existing assignments: {count}");
    }

    public static async Task ImportMpkAssignmentsAsync(
    List<ApprovedStopMatch> matches,
    List<MpkStop> mpkStops,
    bool dryRun = true)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("VELOCITY_DB_CONNECTION")
            ?? throw new InvalidOperationException(
                "Brak zmiennej VELOCITY_DB_CONNECTION.");

        if (matches.GroupBy(m => m.MpkNumber)
                .Any(g => g.Count() > 1) ||
            matches.GroupBy(m => m.VeloCityStopId)
                .Any(g => g.Count() > 1))
        {
            throw new InvalidOperationException(
                "Wykryto powtarzające się przypisania.");
        }

        await using var connection =
            new NpgsqlConnection(connectionString);

        await connection.OpenAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        try
        {
            const string sql = """
                UPDATE "Stops"
                SET
                    "ExternalMpkStopId" = @mpkId,
                    "ExternalMpkStopNumber" = @mpkNumber
                WHERE "Id" = @stopId
                AND "ExternalMpkStopId" IS NULL
                AND "ExternalMpkStopNumber" IS NULL;
                """;

            foreach (var match in matches)
            {
                var mpk = mpkStops.Single(s =>
                    s.Number == match.MpkNumber);

                await using var command =
                    new NpgsqlCommand(sql, connection, transaction);

                command.Parameters.AddWithValue(
                    "mpkId", mpk.Id);

                command.Parameters.AddWithValue(
                    "mpkNumber", mpk.Number);

                command.Parameters.AddWithValue(
                    "stopId", match.VeloCityStopId);

                var affected = await command.ExecuteNonQueryAsync();

                if (affected != 1)
                {
                    throw new InvalidOperationException(
                        $"Nie można przypisać MPK {mpk.Number} " +
                        $"do VeloCity {match.VeloCityStopId}.");
                }

                Console.WriteLine(
                    $"ASSIGNED: MPK {mpk.Number} -> " +
                    $"VeloCity {match.VeloCityStopId}");
            }

            if (dryRun)
            {
                await transaction.RollbackAsync();
                Console.WriteLine("DRY-RUN: Transaction rolled back.");
            }
            else
            {
                await transaction.CommitAsync();
                Console.WriteLine("IMPORT COMMITTED.");
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


public static async Task<Dictionary<string, int>>
    GetMpkStopMappingsAsync()
{
    var connectionString =
        Environment.GetEnvironmentVariable("VELOCITY_DB_CONNECTION")
        ?? throw new InvalidOperationException(
            "Brak zmiennej VELOCITY_DB_CONNECTION.");

    await using var connection =
        new NpgsqlConnection(connectionString);

    await connection.OpenAsync();

    const string sql = """
        SELECT "Id", "ExternalMpkStopNumber"
        FROM "Stops"
        WHERE "ExternalMpkStopNumber" IS NOT NULL
          AND "ExternalMpkStopId" IS NOT NULL;
        """;

    await using var command =
        new NpgsqlCommand(sql, connection);

    await using var reader =
        await command.ExecuteReaderAsync();

    var mappings = new Dictionary<string, int>();

    while (await reader.ReadAsync())
    {
        var stopId = reader.GetInt32(0);
        var mpkNumber = reader.GetString(1);

        if (!mappings.TryAdd(mpkNumber, stopId))
        {
            throw new InvalidOperationException(
                $"Duplikat numeru MPK: {mpkNumber}");
        }
    }

    return mappings;
}

}