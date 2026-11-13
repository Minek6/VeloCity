
using Npgsql;
using VeloCity.RouteImporter.Models;

namespace VeloCity.RouteImporter.Database;

public static class RouteStopRepository
{
    public static async Task ImportAsync(
        List<RouteStopsImport> routes,
        bool dryRun = true)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("VELOCITY_DB_CONNECTION")
            ?? throw new InvalidOperationException(
                "Brak zmiennej VELOCITY_DB_CONNECTION.");

        await using var connection =
            new NpgsqlConnection(connectionString);

        await connection.OpenAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        try
        {
            int importedRoutes = 0;
            int skippedRoutes = 0;
            int insertedStops = 0;

            foreach (var route in routes)
            {
                if (route.Stops.Count == 0 ||
                    route.Stops.Any(s => !s.VeloCityStopId.HasValue))
                {
                    skippedRoutes++;

                    Console.WriteLine(
                        $"BLOCKED: Variant {route.ExternalVariantId} " +
                        "(incomplete stops)");

                    continue;
                }

                const string routeSql = """
                    SELECT "Id", "LineId", "Direction"
                    FROM "Routes"
                    WHERE "ExternalVariantId" = @variantId;
                    """;

                await using var routeCommand =
                    new NpgsqlCommand(
                        routeSql, connection, transaction);

                routeCommand.Parameters.AddWithValue(
                    "variantId", route.ExternalVariantId);

                int routeId;
                int lineId;
                int direction;

                await using (var reader =
                    await routeCommand.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        throw new InvalidOperationException(
                            $"Route not found: {route.ExternalVariantId}");
                    }

                    routeId = reader.GetInt32(0);
                    lineId = reader.GetInt32(1);
                    direction = reader.GetInt32(2);
                }

                if (direction != route.Direction)
                {
                    throw new InvalidOperationException(
                        $"Direction mismatch: {route.ExternalVariantId}");
                }

                const string existingSql = """
                    SELECT COUNT(*)
                    FROM "RouteStops"
                    WHERE "RouteId" = @routeId;
                    """;

                await using var existingCommand =
                    new NpgsqlCommand(
                        existingSql, connection, transaction);

                existingCommand.Parameters.AddWithValue(
                    "routeId", routeId);

                var existingCount = Convert.ToInt32(
                    await existingCommand.ExecuteScalarAsync());

                if (existingCount > 0)
                {
                    skippedRoutes++;

                    Console.WriteLine(
                        $"SKIPPED: Variant {route.ExternalVariantId} " +
                        "(stops already exist)");

                    continue;
                }

                const string insertSql = """
                    INSERT INTO "RouteStops"
                        ("LineId", "StopId", "Sequence",
                         "Direction", "RouteId")
                    VALUES
                        (@lineId, @stopId, @sequence,
                         @direction, @routeId);
                    """;

                foreach (var stop in route.Stops
                    .OrderBy(s => s.Sequence))
                {
                    await using var command =
                        new NpgsqlCommand(
                            insertSql, connection, transaction);

                    command.Parameters.AddWithValue("lineId", lineId);
                    command.Parameters.AddWithValue(
                        "stopId", stop.VeloCityStopId!.Value);
                    command.Parameters.AddWithValue(
                        "sequence", stop.Sequence);
                    command.Parameters.AddWithValue(
                        "direction", direction);
                    command.Parameters.AddWithValue("routeId", routeId);

                    await command.ExecuteNonQueryAsync();
                    insertedStops++;
                }

                importedRoutes++;

                Console.WriteLine(
                    $"IMPORTED: Variant {route.ExternalVariantId} " +
                    $"({route.Stops.Count} stops)");
            }

            Console.WriteLine();
            Console.WriteLine($"Imported routes: {importedRoutes}");
            Console.WriteLine($"Skipped routes: {skippedRoutes}");
            Console.WriteLine($"Inserted stop positions: {insertedStops}");

            if (dryRun)
            {
                await transaction.RollbackAsync();
                Console.WriteLine("DRY-RUN: Transaction rolled back.");
            }
            else
            {
                await transaction.CommitAsync();
                Console.WriteLine("ROUTE STOPS IMPORT COMMITTED.");
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
