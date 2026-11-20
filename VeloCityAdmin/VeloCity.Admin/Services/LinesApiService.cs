using System.Net.Http.Json;
using VeloCity.Admin.Models;
using VeloCity.Admin.Models.Dto;
using VeloCity.Admin.Models.Forms;

namespace VeloCity.Admin.Services;

public sealed class LinesApiService(
    AdminApiClientFactory apiClientFactory)
{
    public async Task<PaginatedResponse<LineDto>> GetAllAsync(
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 20,
        bool isDescending = false,
        bool? isActive = null)
    {
        using var client = await apiClientFactory.CreateAsync();

        var query = new Dictionary<string, string>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["isDescending"] = isDescending ? "true" : "false",
            ["includeInactive"] = "true"
        };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query["searchTerm"] = searchTerm.Trim();
        }

        if (isActive.HasValue)
        {
            query["isActive"] = isActive.Value
                ? "true"
                : "false";
        }

        var url = "Lines?" + string.Join("&",
            query.Select(x =>
                $"{Uri.EscapeDataString(x.Key)}=" +
                $"{Uri.EscapeDataString(x.Value)}"));

        return await client.GetFromJsonAsync<
            PaginatedResponse<LineDto>>(url)
            ?? throw new InvalidOperationException(
                "API zwróciło pustą odpowiedź linii.");
    }

    public async Task<LineDetailsDto?> GetByIdAsync(int id)
    {
        using var client = await apiClientFactory.CreateAsync();

        return await client.GetFromJsonAsync<LineDetailsDto>(
            $"Lines/{id}");
    }

    public async Task UpdateAsync(LineFormModel model)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PutAsJsonAsync(
            $"Lines/{model.Id}",
            new
            {
                Name = model.Name.Trim()
            });

        response.EnsureSuccessStatusCode();
    }

    public async Task DeactivateAsync(int id)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.DeleteAsync(
            $"Lines/{id}");

        response.EnsureSuccessStatusCode();
    }

    public async Task ActivateAsync(int id)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PutAsync(
            $"Lines/{id}/activate",
            content: null);

        response.EnsureSuccessStatusCode();
    }

    public async Task<LineDto> CreateAsync(LineFormModel model)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PostAsJsonAsync(
            "Lines/create",
            new
            {
                Name = model.Name.Trim()
            });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"API error {(int)response.StatusCode}: {error}",
                null,
                response.StatusCode);
        }

        return await response.Content
            .ReadFromJsonAsync<LineDto>()
            ?? throw new InvalidOperationException(
                "API nie zwróciło utworzonej linii.");
    }

}
