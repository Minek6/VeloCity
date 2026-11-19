using System.Net.Http.Json;
using VeloCity.Admin.Models;
using VeloCity.Admin.Models.Dto;

namespace VeloCity.Admin.Services;

public sealed class TicketsApiService(
    AdminApiClientFactory apiClientFactory)
{
    public async Task<PaginatedResponse<AdminTicketDto>>
        GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            int? userId = null,
            int? vehicleId = null,
            bool? isValidated = null,
            string sortBy = "date",
            string sortDirection = "desc")
    {
        using var client = await apiClientFactory.CreateAsync();

        var parameters = new List<string>
        {
            $"PageNumber={pageNumber}",
            $"PageSize={pageSize}",
            $"SortBy={Uri.EscapeDataString(sortBy)}",
            $"SortDirection={Uri.EscapeDataString(sortDirection)}"
        };

        if (userId.HasValue)
            parameters.Add($"UserId={userId.Value}");

        if (vehicleId.HasValue)
            parameters.Add($"VehicleId={vehicleId.Value}");

        if (isValidated.HasValue)
            parameters.Add(
                $"IsValidated={isValidated.Value.ToString().ToLowerInvariant()}");

        var url = $"Tickets?{string.Join("&", parameters)}";

        using var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<PaginatedResponse<AdminTicketDto>>()
            ?? throw new InvalidOperationException(
                "API zwróciło pustą odpowiedź.");
    }

    public async Task<List<TicketValidityAdjustmentDto>>
        GetValidityAdjustmentsAsync(int ticketId)
    {
        using var client = await apiClientFactory.CreateAsync();

        return await client.GetFromJsonAsync<
            List<TicketValidityAdjustmentDto>>(
                $"Tickets/{ticketId}/validity-adjustments")
            ?? [];
    }

    public async Task UpdateValidityAsync(
        int ticketId,
        DateTime validToUtc,
        string reason)
    {
        using var client = await apiClientFactory.CreateAsync();

        var command = new
        {
            TicketId = ticketId,
            ValidTo = validToUtc,
            Reason = reason.Trim()
        };

        using var response = await client.PutAsJsonAsync(
            $"Tickets/{ticketId}/validity",
            command);

        response.EnsureSuccessStatusCode();
    }

}
