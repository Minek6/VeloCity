using System.Net.Http.Json;
using VeloCity.Admin.Models.Dto;
using VeloCity.Admin.Services;
using VeloCity.Admin.Models.Forms;

namespace VeloCity.Admin.Services;

public sealed class TicketTypesApiService(
    AdminApiClientFactory apiClientFactory)
{
    public async Task<List<TicketTypeDto>> GetAllAsync()
    {
        using var client = await apiClientFactory.CreateAsync();

        return await client.GetFromJsonAsync<List<TicketTypeDto>>(
            "Tickets/types/admin") ?? [];
    }

    public async Task CreateAsync(CreateTicketTypeFormModel model)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PostAsJsonAsync(
            "Tickets/types",
            new
            {
                Name = model.Name.Trim(),
                Price = model.Price,
                DurationInMinutes = model.DurationInMinutes,
                ZoneLimit = model.ZoneLimit
            });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"API error {(int)response.StatusCode}: {error}",
                null,
                response.StatusCode);
        }
    }

    public async Task UpdateAsync(
        int id,
        CreateTicketTypeFormModel model)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PutAsJsonAsync(
            $"Tickets/types/{id}",
            new
            {
                Id = id,
                Name = model.Name.Trim(),
                Price = model.Price,
                DurationInMinutes = model.DurationInMinutes,
                ZoneLimit = model.ZoneLimit
            });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"API error {(int)response.StatusCode}: {error}",
                null,
                response.StatusCode);
        }
    }

    public async Task DeactivateAsync(int id)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.DeleteAsync(
            $"Tickets/types/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"Nie udało się dezaktywować rodzaju biletu: {error}",
                null,
                response.StatusCode);
        }
    }

    public async Task ActivateAsync(int id)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PatchAsync(
            $"Tickets/types/{id}/activate",
            content: null);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"Nie udało się aktywować rodzaju biletu: {error}",
                null,
                response.StatusCode);
        }
    }

}
