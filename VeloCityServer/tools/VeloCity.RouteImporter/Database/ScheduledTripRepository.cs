
using System.Text.Json;
using Npgsql;

namespace VeloCity.RouteImporter.Database;

public static class ScheduledTripRepository
{
    public static async Task ImportAsync(
        string filePath,
        bool commit = false)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("VELOCITY_DB_CONNECTION")
            ?? throw new InvalidOperationException(
                "Brak VELOCITY_DB_CONNECTION.");

        using var document = JsonDocument.Parse(
            await File.ReadAllTextAsync(filePath));

        var variants = document.RootElement
            .GetProperty("variants")
            .EnumerateArray()
            .ToArray();

        var runs = variants
            .SelectMany(v => v.GetProperty("runs").EnumerateArray()
                .Select(r => (
                    VariantId: v.GetProperty("id").GetInt32(),
                    Run: r)))
            .ToList();

        Console.WriteLine($"Source runs: {runs.Count}");

        if (runs.Count != 48)
            throw new InvalidOperationException(
                $"Oczekiwano 48 kursów, otrzymano {runs.Count}.");

        if (runs.Select(x => x.Run.GetProperty("id").GetInt32())
                .Distinct().Count() != runs.Count)
            throw new InvalidOperationException(
                "Powtarzające się identyfikatory kursów MPK.");

        await using var connection =
            new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        NpgsqlCommand Command(string sql)
        {
            var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = sql;
            return cmd;
        }

        async Task<int> MaxId(string table)
        {
            // Nazwy tabel są stałe i zdefiniowane w kodzie.
            await using var cmd = Command(
                $"SELECT COALESCE(MAX(\"Id\"), 0) FROM \"{table}\"");
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        // Blokada zapobiega równoległemu przydzieleniu
        // tych samych identyfikatorów podczas importu.
        await using (var cmd = Command("""
            LOCK TABLE
                "ServiceCalendars",
                "ScheduledTrips",
                "ScheduledStopTimes"
            IN SHARE ROW EXCLUSIVE MODE;
            """))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        var calendarId = await MaxId("ServiceCalendars");
        var tripId = await MaxId("ScheduledTrips");
        var stopTimeId = await MaxId("ScheduledStopTimes");

        var calendarCache = new Dictionary<string, int>();
        var imported = 0;
        var skipped = 0;
        var insertedTimes = 0;

        var validFrom = new DateTime(2026, 9, 21);
        var validTo = new DateTime(2026, 10, 23);

        foreach (var item in runs)
        {
            var run = item.Run;
            var externalRunId = run.GetProperty("id").GetInt32();

            // 1. Znajdź trasę tego kursu.
            int routeId;

            await using (var cmd = Command("""
                SELECT "Id"
                FROM "Routes"
                WHERE "ExternalVariantId" = @variantId;
                """))
            {
                cmd.Parameters.AddWithValue(
                    "variantId", item.VariantId);

                var value = await cmd.ExecuteScalarAsync();
                if (value == null)
                    throw new InvalidOperationException(
                        $"Brak trasy {item.VariantId}.");

                routeId = Convert.ToInt32(value);
            }

            // 2. Nie importuj ponownie istniejącego kursu.
            await using (var cmd = Command("""
                SELECT "RouteId"
                FROM "ScheduledTrips"
                WHERE "ExternalRunId" = @runId;
                """))
            {
                cmd.Parameters.AddWithValue("runId", externalRunId);

                var existing = await cmd.ExecuteScalarAsync();

                if (existing != null)
                {
                    if (Convert.ToInt32(existing) != routeId)
                        throw new InvalidOperationException(
                            $"Kurs {externalRunId} jest przypisany " +
                            "do innej trasy.");

                    skipped++;
                    continue;
                }
            }

            // 3. Odczytaj właściwe pozycje RouteStops.
            var routeStops = new List<(int Id, string Number)>();

            await using (var cmd = Command("""
                SELECT rs."Id", s."ExternalMpkStopNumber"
                FROM "RouteStops" rs
                JOIN "Stops" s ON s."Id" = rs."StopId"
                WHERE rs."RouteId" = @routeId
                ORDER BY rs."Sequence", rs."Id";
                """))
            {
                cmd.Parameters.AddWithValue("routeId", routeId);

                await using var reader =
                    await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    routeStops.Add((
                        reader.GetInt32(0),
                        reader.GetString(1)));
                }
            }

            var sourceStops = run.GetProperty("stops")
                .EnumerateArray()
                .Where(s => s.TryGetProperty("stop_order", out _))
                .OrderBy(s => s.GetProperty("stop_order").GetInt32())
                .ToList();

            if (sourceStops.Count != routeStops.Count)
                throw new InvalidOperationException(
                    $"Kurs {externalRunId}: inna liczba przystanków.");

            for (var i = 0; i < sourceStops.Count; i++)
            {
                var number =
                    sourceStops[i].GetProperty("number").GetString();

                var order =
                    sourceStops[i].GetProperty("stop_order").GetInt32();

                if (order != i || number != routeStops[i].Number)
                    throw new InvalidOperationException(
                        $"Kurs {externalRunId}: " +
                        $"niezgodność przystanku na pozycji {i}.");
            }

            // 4. Odczytaj dni kursowania.
            string[] dayFields =
            {
                "is_monday", "is_tuesday", "is_wednesday",
                "is_thursday", "is_friday",
                "is_saturday", "is_sunday"
            };

            var days = dayFields
                .Select(name =>
                    run.GetProperty(name).GetInt32() == 1)
                .ToArray();

            if (!days.Any(x => x))
                throw new InvalidOperationException(
                    $"Kurs {externalRunId} nie ma dni kursowania.");

            var mask = string.Concat(
                days.Select(x => x ? "1" : "0"));

            var calendarName = $"MPK86-{mask}";

            // 5. Znajdź albo utwórz historyczny kalendarz.
            if (!calendarCache.TryGetValue(mask, out var serviceId))
            {
                await using (var cmd = Command("""
                    SELECT "Id"
                    FROM "ServiceCalendars"
                    WHERE "Name" = @name
                      AND "ValidFrom" = @from
                      AND "ValidTo" = @to
                      AND "Monday" = @d0
                      AND "Tuesday" = @d1
                      AND "Wednesday" = @d2
                      AND "Thursday" = @d3
                      AND "Friday" = @d4
                      AND "Saturday" = @d5
                      AND "Sunday" = @d6;
                    """))
                {
                    cmd.Parameters.AddWithValue("name", calendarName);
                    cmd.Parameters.AddWithValue("from", validFrom);
                    cmd.Parameters.AddWithValue("to", validTo);

                    for (var i = 0; i < 7; i++)
                        cmd.Parameters.AddWithValue($"d{i}", days[i]);

                    var existing = await cmd.ExecuteScalarAsync();
                    serviceId = existing == null
                        ? 0
                        : Convert.ToInt32(existing);
                }

                if (serviceId == 0)
                {
                    serviceId = ++calendarId;

                    await using var cmd = Command("""
                        INSERT INTO "ServiceCalendars" (
                            "Id", "Name", "ValidFrom", "ValidTo",
                            "Monday", "Tuesday", "Wednesday",
                            "Thursday", "Friday", "Saturday", "Sunday"
                        )
                        VALUES (
                            @id, @name, @from, @to,
                            @d0, @d1, @d2, @d3, @d4, @d5, @d6
                        );
                        """);

                    cmd.Parameters.AddWithValue("id", serviceId);
                    cmd.Parameters.AddWithValue("name", calendarName);
                    cmd.Parameters.AddWithValue("from", validFrom);
                    cmd.Parameters.AddWithValue("to", validTo);

                    for (var i = 0; i < 7; i++)
                        cmd.Parameters.AddWithValue($"d{i}", days[i]);

                    await cmd.ExecuteNonQueryAsync();
                }

                calendarCache[mask] = serviceId;
            }

            // 6. Utwórz ScheduledTrip.
            var newTripId = ++tripId;

            await using (var cmd = Command("""
                INSERT INTO "ScheduledTrips" (
                    "Id", "RouteId",
                    "ServiceCalendarId", "ExternalRunId"
                )
                VALUES (@id, @route, @calendar, @external);
                """))
            {
                cmd.Parameters.AddWithValue("id", newTripId);
                cmd.Parameters.AddWithValue("route", routeId);
                cmd.Parameters.AddWithValue("calendar", serviceId);
                cmd.Parameters.AddWithValue("external", externalRunId);

                await cmd.ExecuteNonQueryAsync();
            }

            // 7. Godziny odjazdów.
            var startMinutes =
                run.GetProperty("start_time").GetInt32();

            var totalMinutes =
                run.GetProperty("total_time").GetInt32();

            var cumulativeSeconds = 0;

            for (var i = 0; i < sourceStops.Count; i++)
            {
                cumulativeSeconds += sourceStops[i]
                    .GetProperty("stop_time").GetInt32();

                var departure = TimeSpan.FromMinutes(startMinutes)
                    + TimeSpan.FromSeconds(cumulativeSeconds);

                await using var cmd = Command("""
                    INSERT INTO "ScheduledStopTimes" (
                        "Id", "ScheduledTripId",
                        "RouteStopId", "ArrivalTime", "DepartureTime"
                    )
                    VALUES (@id, @trip, @stop, @arrival, @departure);
                    """);

                cmd.Parameters.AddWithValue("id", ++stopTimeId);
                cmd.Parameters.AddWithValue("trip", newTripId);
                cmd.Parameters.AddWithValue(
                    "stop", routeStops[i].Id);
                cmd.Parameters.AddWithValue("arrival", departure);
                cmd.Parameters.AddWithValue("departure", departure);

                await cmd.ExecuteNonQueryAsync();
                insertedTimes++;
            }

            if (cumulativeSeconds != totalMinutes * 60)
                throw new InvalidOperationException(
                    $"Kurs {externalRunId}: błędny czas przejazdu.");

            imported++;
        }

        Console.WriteLine();
        Console.WriteLine("=== ScheduledTrips import ===");
        Console.WriteLine($"Imported: {imported}");
        Console.WriteLine($"Skipped: {skipped}");
        Console.WriteLine($"Inserted stop times: {insertedTimes}");

        if (commit)
        {
            await transaction.CommitAsync();
            Console.WriteLine("SCHEDULE IMPORT COMMITTED.");
        }
        else
        {
            await transaction.RollbackAsync();
            Console.WriteLine("DRY-RUN: Transaction rolled back.");
        }
    }
}
