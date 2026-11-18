using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace VeloCity.Admin.Services;

public sealed class AdminApiClientFactory(
    IHttpClientFactory httpClientFactory,
    AuthenticationStateProvider authenticationStateProvider,
    IMemoryCache cache)
{
    public async Task<HttpClient> CreateAsync()
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
}
