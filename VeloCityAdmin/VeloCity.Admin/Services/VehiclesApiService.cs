using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;
using VeloCity.Admin.Models;

namespace VeloCity.Admin.Services;

public sealed class VehiclesApiService(
    IHttpClientFactory httpClientFactory,
    AuthenticationStateProvider authenticationStateProvider,
    IMemoryCache cache)
{
    private async Task<HttpClient> CreateAuthorizedClientAsync()
    {
        var authState = await authenticationStateProvider
            .GetAuthenticationStateAsync();

        var user = authState.User;

        if (user.Identity?.IsAuthenticated != true ||
            !user.IsInRole("Admin"))
        {
            throw new UnauthorizedAccessException(
                "Administrator is not authenticated.");
        }

        var sessionId = user.FindFirst("AdminSessionId")?.Value;

        if (string.IsNullOrWhiteSpace(sessionId) ||
            !cache.TryGetValue(
                $"admin-api-token:{sessionId}",
                out string? token) ||
            string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "API session expired. Please sign in again.");
        }

        var client = httpClientFactory.CreateClient("VeloCityApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    public async Task<List<VehicleDto>> GetAllAsync()
    {
        using var client = await CreateAuthorizedClientAsync();

        return await client.GetFromJsonAsync<List<VehicleDto>>(
            "Vehicles/all") ?? [];
    }

    public async Task<VehicleDto?> GetByIdAsync(int id)
    {
        using var client = await CreateAuthorizedClientAsync();

        return await client.GetFromJsonAsync<VehicleDto>(
            $"Vehicles/{id}");
    }

    
    public async Task<VehicleDto> CreateAsync(
        VehicleFormModel model)
    {
        using var client = await CreateAuthorizedClientAsync();

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
        using var client = await CreateAuthorizedClientAsync();

        using var response = await client.PutAsJsonAsync(
            $"Vehicles/{model.Id}",
            new
            {
                Id = model.Id,
                SideNumber = model.SideNumber.Trim(),
                Model = model.Model.Trim()
            });

        response.EnsureSuccessStatusCode();
    }

    public async Task DeactivateAsync(int id)
    {
        using var client = await CreateAuthorizedClientAsync();

        using var response = await client.DeleteAsync(
            $"Vehicles/{id}");

        response.EnsureSuccessStatusCode();
    }

    public async Task ActivateAsync(int id)
    {
        using var client = await CreateAuthorizedClientAsync();

        using var response = await client.PutAsync(
            $"Vehicles/{id}/activate",
            content: null);

        response.EnsureSuccessStatusCode();
    }
}
