using System.Net.Http.Json;
using VeloCity.Admin.Models;
using VeloCity.Admin.Models.Forms;
using VeloCity.Admin.Models.Dto;

namespace VeloCity.Admin.Services;

public sealed class StopsApiService(
    AdminApiClientFactory apiClientFactory)
{
    public async Task<PaginatedResponse<StopDto>> GetAllAsync(
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 20,
        bool? isActive = null)
    {
        using var client = await apiClientFactory.CreateAsync();

        var query = new Dictionary<string, string>
        {
            ["PageNumber"] = pageNumber.ToString(),
            ["PageSize"] = pageSize.ToString(),
            ["SortDirection"] = "asc",
            ["IncludeInactive"] = "true"
        };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query["SearchTerm"] = searchTerm.Trim();
        }

        if (isActive.HasValue)
        {
            query["IsActive"] = isActive.Value
                ? "true"
                : "false";
        }

        var url = "Stops?" + string.Join("&",
            query.Select(x =>
                $"{Uri.EscapeDataString(x.Key)}=" +
                $"{Uri.EscapeDataString(x.Value)}"));

        return await client.GetFromJsonAsync<
            PaginatedResponse<StopDto>>(url)
            ?? throw new InvalidOperationException(
                "API zwróciło pustą odpowiedź.");
    }

    public async Task<StopDto?> GetByIdAsync(int id)
    {
        using var client = await apiClientFactory.CreateAsync();

        return await client.GetFromJsonAsync<StopDto>(
            $"Stops/{id}");
    }

    public async Task<int> CreateAsync(StopFormModel model)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PostAsJsonAsync(
            "Stops",
            new
            {
                Name = model.Name.Trim(),
                Latitude = model.Latitude!.Value,
                Longitude = model.Longitude!.Value,
                Zone = model.Zone!.Value,
                ExternalId = model.ExternalId!.Value
            });

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<int>();
    }

    public async Task UpdateAsync(StopFormModel model)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PutAsJsonAsync(
            $"Stops/{model.Id}",
            new
            {
                Id = model.Id,
                Name = model.Name.Trim(),
                Latitude = model.Latitude!.Value,
                Longitude = model.Longitude!.Value,
                Zone = model.Zone!.Value,
                ExternalId = model.ExternalId!.Value
            });

        response.EnsureSuccessStatusCode();
    }

    public async Task ActivateAsync(int id)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PutAsync(
            $"Stops/{id}/activate",
            content: null);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeactivateAsync(int id)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.DeleteAsync(
            $"Stops/{id}");

        response.EnsureSuccessStatusCode();
    }

}
