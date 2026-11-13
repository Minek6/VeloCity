using Npgsql;
using VeloCity.RouteImporter.Models;

namespace VeloCity.RouteImporter.Database;

public static class RouteRepository
{
    private static string GetConnectionString() =>
        Environment.GetEnvironmentVariable("VELOCITY_DB_CONNECTION")
        ?? throw new InvalidOperationException("Brak zmiennej VELOCITY_DB_CONNECTION.");

    public static async Task CheckRoutesAsync(string lineName)
    {
        await using var connection = new NpgsqlConnection(GetConnectionString());
        await connection.OpenAsync();

        const string sql = """
            SELECT l."Id", l."Name", r."Id", r."Name", r."Direction", r."ExternalVariantId", r."IsActive"
            FROM "Lines" l
            LEFT JOIN "Routes" r ON r."LineId" = l."Id"
            WHERE l."Name" = @lineName
            ORDER BY r."Id";
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("lineName", lineName);
        await using var reader = await command.ExecuteReaderAsync();

        Console.WriteLine("=== Existing VeloCity routes ===");
        int count = 0;
        bool lineFound = false;
        while (await reader.ReadAsync())
        {
            lineFound = true;
            if (reader.IsDBNull(2))
            {
                Console.WriteLine($"Line {reader.GetString(1)} (ID: {reader.GetInt32(0)}) has no routes.");
                continue;
            }

            count++;
            Console.WriteLine(
                $"Route ID: {reader.GetInt32(2)} | {reader.GetString(3)} | " +
                $"Direction: {reader.GetInt32(4)} | " +
                $"MPK Variant: {(reader.IsDBNull(5) ? "NULL" : reader.GetInt32(5).ToString())} | " +
                $"IsActive: {reader.GetBoolean(6)}");
        }

        if (!lineFound)
            Console.WriteLine($"Line {lineName} not found.");

        Console.WriteLine($"Existing routes: {count}");
    }

    public static async Task ImportRoutesAsync(
        string lineName,
        List<MpkRouteVariant> variants,
        bool dryRun = true)
    {
        var selected = variants.Where(v => v.RunsCount > 0).ToList();
        if (selected.Count == 0)
            throw new InvalidOperationException("Brak wariantów zawierających kursy.");
        if (selected.GroupBy(v => v.Id).Any(g => g.Count() > 1))
            throw new InvalidOperationException("Powtórzone identyfikatory wariantów MPK.");

        await using var connection = new NpgsqlConnection(GetConnectionString());
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            const string findLineSql = """
                SELECT "Id" FROM "Lines" WHERE "Name" = @lineName;
                """;
            await using var lineCommand = new NpgsqlCommand(findLineSql, connection, transaction);
            lineCommand.Parameters.AddWithValue("lineName", lineName);
            var lineValue = await lineCommand.ExecuteScalarAsync();
            if (lineValue is not int lineId)
                throw new InvalidOperationException($"Nie znaleziono linii {lineName}.");

            int inserted = 0;
            int skipped = 0;

            foreach (var variant in selected)
            {
                const string existingSql = """
                    SELECT "Id", "LineId", "Name", "Direction"
                    FROM "Routes" WHERE "ExternalVariantId" = @variantId;
                    """;
                await using (var existingCommand = new NpgsqlCommand(existingSql, connection, transaction))
                {
                    existingCommand.Parameters.AddWithValue("variantId", variant.Id);
                    await using var reader = await existingCommand.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        int existingId = reader.GetInt32(0);
                        int existingLineId = reader.GetInt32(1);
                        string existingName = reader.GetString(2);
                        int existingDirection = reader.GetInt32(3);
                        if (existingLineId != lineId || existingName != variant.Name ||
                            existingDirection != variant.Direction)
                        {
                            throw new InvalidOperationException(
                                $"Konflikt wariantu {variant.Id}: istniejący Route {existingId} ma inne dane.");
                        }

                        skipped++;
                        Console.WriteLine($"SKIPPED: Variant {variant.Id} -> Route {existingId} (already exists)");
                        continue;
                    }
                }

                const string insertSql = """
                    INSERT INTO "Routes" ("LineId", "Name", "Direction", "IsActive", "ExternalVariantId")
                    VALUES (@lineId, @name, @direction, false, @variantId)
                    ON CONFLICT ("ExternalVariantId") WHERE "ExternalVariantId" IS NOT NULL
                    DO NOTHING RETURNING "Id";
                    """;
                await using var insertCommand = new NpgsqlCommand(insertSql, connection, transaction);
                insertCommand.Parameters.AddWithValue("lineId", lineId);
                insertCommand.Parameters.AddWithValue("name", variant.Name);
                insertCommand.Parameters.AddWithValue("direction", variant.Direction);
                insertCommand.Parameters.AddWithValue("variantId", variant.Id);

                var result = await insertCommand.ExecuteScalarAsync();
                if (result is not int routeId)
                    throw new InvalidOperationException($"Nie udało się wstawić wariantu {variant.Id}; sprawdź konflikt ID.");

                inserted++;
                Console.WriteLine($"INSERTED: Variant {variant.Id} -> Route {routeId} (inactive)");
            }

            Console.WriteLine($"Inserted: {inserted}; Skipped: {skipped}");
            if (dryRun)
            {
                await transaction.RollbackAsync();
                Console.WriteLine("DRY-RUN: Transaction rolled back.");
            }
            else
            {
                await transaction.CommitAsync();
                Console.WriteLine("ROUTES IMPORT COMMITTED.");
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
