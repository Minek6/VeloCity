using System.Diagnostics;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Features.Dashboard.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Dashboard.Queries.GetSystemHealth;

public sealed class GetSystemHealthHandler(
    ApplicationDbContext context,
    ILogger<GetSystemHealthHandler> logger)
    : IRequestHandler<GetSystemHealthQuery, SystemHealthDto>
{
    public async Task<SystemHealthDto> Handle(
        GetSystemHealthQuery request,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var databaseConnected = false;

        using var timeout = CancellationTokenSource
            .CreateLinkedTokenSource(ct);

        timeout.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            databaseConnected = await context.Database
                .CanConnectAsync(timeout.Token);
        }
        catch (OperationCanceledException)
            when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(
                "PostgreSQL health check timed out.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "PostgreSQL health check failed.");
        }

        stopwatch.Stop();

        return new SystemHealthDto(
            DatabaseConnected: databaseConnected,
            DatabaseResponseTimeMs: stopwatch.ElapsedMilliseconds,
            CheckedAtUtc: DateTime.UtcNow);
    }
}
