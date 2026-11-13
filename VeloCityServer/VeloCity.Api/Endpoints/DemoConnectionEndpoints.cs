
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace VeloCity.Api.Endpoints;

public static class DemoConnectionEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/connections/search",
            async (
                int fromStopId,
                int toStopId,
                IConfiguration config,
                CancellationToken cancellationToken) =>
            {
                if (config["Timetable:Mode"] != "Demo")
                    return Results.NotFound(
                        "Demonstracyjny rozkład jest wyłączony.");

                if (fromStopId <= 0 || toStopId <= 0
                    || fromStopId == toStopId)
                    return Results.BadRequest(
                        "Wybierz dwa różne przystanki.");

                var serviceDateText =
                    config["Timetable:DemoServiceDate"]
                    ?? "2026-09-21";

                if (!DateOnly.TryParseExact(
                    serviceDateText,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var serviceDate))
                    return Results.Problem(
                        "Nieprawidłowa data demonstracyjna.");

                var lineId = config.GetValue<int>(
                    "Timetable:DefaultLineId", 8);

                var connectionString =
                    config.GetConnectionString("DefaultConnection")
                    ?? Environment.GetEnvironmentVariable(
                        "VELOCITY_DB_CONNECTION");

                if (string.IsNullOrWhiteSpace(connectionString))
                    return Results.Problem(
                        "Nie skonfigurowano połączenia z bazą.");

                const string sql = """
                    SELECT
                        st."ExternalRunId",
                        r."ExternalVariantId",
                        s1."Id" AS "FromStopId",
                        s1."Name" AS "FromName",
                        t1."DepartureTime",
                        s2."Id" AS "ToStopId",
                        s2."Name" AS "ToName",
                        t2."ArrivalTime",
                        st."Id" AS "ScheduledTripId"
                    FROM "ScheduledTrips" st
                    JOIN "Routes" r
                        ON r."Id" = st."RouteId"
                    JOIN "ServiceCalendars" sc
                        ON sc."Id" = st."ServiceCalendarId"
                    JOIN "ScheduledStopTimes" t1
                        ON t1."ScheduledTripId" = st."Id"
                    JOIN "RouteStops" rs1
                        ON rs1."Id" = t1."RouteStopId"
                    JOIN "Stops" s1
                        ON s1."Id" = rs1."StopId"
                    JOIN "ScheduledStopTimes" t2
                        ON t2."ScheduledTripId" = st."Id"
                    JOIN "RouteStops" rs2
                        ON rs2."Id" = t2."RouteStopId"
                    JOIN "Stops" s2
                        ON s2."Id" = rs2."StopId"
                    WHERE r."LineId" = @lineId
                      AND s1."Id" = @fromStopId
                      AND s2."Id" = @toStopId
                      AND rs1."Sequence" < rs2."Sequence"
                      AND sc."ValidFrom" <= @serviceDate
                      AND sc."ValidTo" >= @serviceDate
                      AND (
                          EXISTS (
                              SELECT 1
                              FROM "ServiceCalendarExceptions" ex
                              WHERE ex."ServiceCalendarId" = sc."Id"
                                AND ex."Date" = @serviceDate
                                AND ex."IsAdded" = TRUE
                          )
                          OR (
                              CASE EXTRACT(
                                  ISODOW FROM @serviceDate::date
                              )::int
                                  WHEN 1 THEN sc."Monday"
                                  WHEN 2 THEN sc."Tuesday"
                                  WHEN 3 THEN sc."Wednesday"
                                  WHEN 4 THEN sc."Thursday"
                                  WHEN 5 THEN sc."Friday"
                                  WHEN 6 THEN sc."Saturday"
                                  WHEN 7 THEN sc."Sunday"
                              END
                              AND NOT EXISTS (
                                  SELECT 1
                                  FROM "ServiceCalendarExceptions" ex
                                  WHERE ex."ServiceCalendarId" = sc."Id"
                                    AND ex."Date" = @serviceDate
                                    AND ex."IsAdded" = FALSE
                              )
                          )
                      )
                    ORDER BY
                        t1."DepartureTime",
                        st."ExternalRunId";
                    """;

                var results = new List<object>();

                await using var connection =
                    new NpgsqlConnection(connectionString);

                await connection.OpenAsync(cancellationToken);

                await using var command =
                    new NpgsqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "lineId", lineId);
                command.Parameters.AddWithValue(
                    "fromStopId", fromStopId);
                command.Parameters.AddWithValue(
                    "toStopId", toStopId);
                command.Parameters.Add(
                    "serviceDate", NpgsqlDbType.Date
                ).Value = serviceDate;

                await using var reader =
                    await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    var departure = reader.GetTimeSpan(4);
                    var arrival = reader.GetTimeSpan(7);

                    results.Add(new
                    {
                        lineNumber = "6",
                        externalRunId = reader.GetInt32(0),
                        externalVariantId = reader.GetInt32(1),
                        fromStopId = reader.GetInt32(2),
                        fromStopName = reader.GetString(3),
                        departureTime =
                            departure.ToString(@"hh\:mm"),
                        toStopId = reader.GetInt32(5),
                        toStopName = reader.GetString(6),
                        arrivalTime =
                            arrival.ToString(@"hh\:mm"),
                        durationMinutes =
                            (arrival - departure).TotalMinutes,
                        scheduledTripId = reader.GetInt32(8),
                    });
                }

                return Results.Ok(new
                {
                    mode = "Demo",
                    lineNumber = "6",
                    timetableId = 86,
                    serviceDate,
                    count = results.Count,
                    connections = results
                });
            })
            .WithTags("Connections")
            .WithName("SearchDemoConnections");
    }
}
