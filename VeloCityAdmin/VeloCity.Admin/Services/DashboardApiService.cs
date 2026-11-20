using System.Net.Http.Json;
using VeloCity.Admin.Models.Dto;
using System.Diagnostics;

namespace VeloCity.Admin.Services;

public sealed class DashboardApiService(
    AdminApiClientFactory apiClientFactory)
{
    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        using var client = await apiClientFactory.CreateAsync();

        return await client.GetFromJsonAsync<DashboardStatsDto>(
            "Dashboard/stats")
            ?? throw new InvalidOperationException(
                "Nie udało się pobrać statystyk dashboardu.");
    }

    public async Task<List<DailyTicketSalesDto>>
        GetTicketSalesAsync(int days = 7)
    {
        using var client = await apiClientFactory.CreateAsync();

        return await client.GetFromJsonAsync<List<DailyTicketSalesDto>>(
            $"Dashboard/ticket-sales?days={days}")
            ?? throw new InvalidOperationException(
                "Nie udało się pobrać danych sprzedaży.");
    }

    public async Task<List<DailyTicketSalesDto>> GetTicketSalesReportAsync(
        DateOnly from,
        DateOnly to,
        int? ticketTypeId = null,
        bool? isValidated = null)
    {
        using var client = await apiClientFactory.CreateAsync();

        var parameters = new List<string>
        {
            $"from={from:yyyy-MM-dd}",
            $"to={to:yyyy-MM-dd}"
        };

        if (ticketTypeId.HasValue)
        {
            parameters.Add($"ticketTypeId={ticketTypeId.Value}");
        }

        if (isValidated.HasValue)
        {
            parameters.Add(
                $"isValidated={isValidated.Value.ToString().ToLowerInvariant()}");
        }

        var url = "Dashboard/ticket-sales/report?"
                + string.Join("&", parameters);

        return await client.GetFromJsonAsync<List<DailyTicketSalesDto>>(url)
            ?? throw new InvalidOperationException(
                "API zwróciło pustą odpowiedź raportu.");
    }

    public async Task<List<TicketTypeSalesDto>> GetTicketTypeRankingAsync(
        DateOnly from,
        DateOnly to,
        int? ticketTypeId = null,
        bool? isValidated = null)
    {
        using var client = await apiClientFactory.CreateAsync();

        var parameters = new List<string>
        {
            $"from={from:yyyy-MM-dd}",
            $"to={to:yyyy-MM-dd}"
        };

        if (ticketTypeId.HasValue)
            parameters.Add($"ticketTypeId={ticketTypeId.Value}");

        if (isValidated.HasValue)
            parameters.Add(
                $"isValidated={isValidated.Value.ToString().ToLowerInvariant()}");

        var url = "Dashboard/ticket-sales/ranking?"
            + string.Join("&", parameters);

        return await client.GetFromJsonAsync<List<TicketTypeSalesDto>>(url)
            ?? throw new InvalidOperationException(
                "API zwróciło pustą odpowiedź rankingu.");
    }

    public async Task<(SystemHealthDto Data, long ResponseTimeMs)>
        GetSystemHealthAsync()
    {
        using var client = await apiClientFactory.CreateAsync();

        using var timeout = new CancellationTokenSource(
            TimeSpan.FromSeconds(8));

        var stopwatch = Stopwatch.StartNew();

        using var response = await client.GetAsync(
            "Dashboard/system-health",
            timeout.Token);

        stopwatch.Stop();

        response.EnsureSuccessStatusCode();

        var data = await response.Content
            .ReadFromJsonAsync<SystemHealthDto>(
                cancellationToken: timeout.Token)
            ?? throw new InvalidOperationException(
                "API zwróciło pustą odpowiedź monitora.");

        return (data, stopwatch.ElapsedMilliseconds);
    }

}
