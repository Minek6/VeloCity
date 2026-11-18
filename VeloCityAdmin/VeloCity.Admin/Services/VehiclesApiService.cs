using System.Net.Http.Json;
using VeloCity.Admin.Models.Dto;
using VeloCity.Admin.Models;

namespace VeloCity.Admin.Services;

public sealed class VehiclesApiService(
    AdminApiClientFactory apiClientFactory)
{
    public async Task<List<VehicleDto>> GetAllAsync()
    {
        using var client = await apiClientFactory.CreateAsync();

        return await client.GetFromJsonAsync<List<VehicleDto>>(
            "Vehicles/all") ?? [];
    }

    public async Task<VehicleDto?> GetByIdAsync(int id)
    {
        using var client = await apiClientFactory.CreateAsync();

        return await client.GetFromJsonAsync<VehicleDto>(
            $"Vehicles/{id}");
    }

    
    public async Task<VehicleDto> CreateAsync(
        VehicleFormModel model)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PostAsJsonAsync(
            "Vehicles",
            new
            {
                SideNumber = model.SideNumber.Trim(),
                Model = model.Model.Trim()
            });

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<VehicleDto>()
            ?? throw new InvalidOperationException(
                "API did not return the created vehicle.");
    }

    public async Task UpdateAsync(VehicleFormModel model)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PutAsJsonAsync(
            $"Vehicles/{model.Id}",
            new
            {
                Id = model.Id,
                SideNumber = model.SideNumber.Trim(),
                Model = model.Model.Trim()
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
            $"Vehicles/{id}");

        response.EnsureSuccessStatusCode();
    }

    public async Task ActivateAsync(int id)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PutAsync(
            $"Vehicles/{id}/activate",
            content: null);

        response.EnsureSuccessStatusCode();
    }
}
