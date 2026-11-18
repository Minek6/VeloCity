using System.Net.Http.Json;
using System.Text.Json;
using System.Net;
using VeloCity.Admin.Models;
using VeloCity.Admin.Models.Dto;
using VeloCity.Admin.Models.Enums;
using VeloCity.Admin.Models.Forms;

namespace VeloCity.Admin.Services;

public sealed class UsersApiService(
    AdminApiClientFactory apiClientFactory
)
{
    public async Task<PaginatedResponse<UserDto>> GetUsersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null,
        bool? isActive = null,
        string sortDirection = "asc")
    {
        using var client = await apiClientFactory.CreateAsync();

        var query = new List<string>
        {
            $"PageNumber={pageNumber}",
            $"PageSize={pageSize}",
            $"SortDirection={Uri.EscapeDataString(sortDirection)}"
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add(
                $"Search={Uri.EscapeDataString(search.Trim())}");
        }

        if (isActive.HasValue)
        {
            query.Add(
                $"IsActive={isActive.Value.ToString().ToLowerInvariant()}");
        }

        var url = $"Users?{string.Join("&", query)}";

        return await client.GetFromJsonAsync<PaginatedResponse<UserDto>>(url)
            ?? throw new InvalidOperationException(
                "API did not return the users list.");
    }

    public async Task<UserDetailsDto> GetByIdAsync(int id)
    {
        using var client = await apiClientFactory.CreateAsync();

        return await client.GetFromJsonAsync<UserDetailsDto>(
            $"Users/{id}")
            ?? throw new InvalidOperationException(
                "API did not return user details.");
    }

    public async Task UpdateStatusAsync(
        int id,
        UserRole role,
        bool isActive)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PutAsJsonAsync(
            $"Users/{id}/status",
            new
            {
                Role = role,
                IsActive = isActive
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

    public async Task UpdateProfileAsync(
        int id,
        string name,
        string surname,
        string email)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PutAsJsonAsync(
            $"Users/{id}/profile",
            new
            {
                Name = name.Trim(),
                Surname = surname.Trim(),
                Email = email.Trim()
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

    public async Task<BalanceAdjustmentDto> AdjustBalanceAsync(
        int userId,
        decimal amount,
        string reason)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PostAsJsonAsync(
            $"Users/{userId}/balance-adjustments",
            new
            {
                Amount = amount,
                Reason = reason.Trim()
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
            .ReadFromJsonAsync<BalanceAdjustmentDto>()
            ?? throw new InvalidOperationException(
                "API did not return the balance adjustment.");
    }

    public async Task CreateUserAsync(CreateUserFormModel model)
    {
        using var client = await apiClientFactory.CreateAsync();

        using var response = await client.PostAsJsonAsync(
            "Users/admin",
            new
            {
                Name = model.Name.Trim(),
                Surname = model.Surname.Trim(),
                Email = model.Email.Trim(),
                Password = model.Password,
                Role = (int)model.Role
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    try
                    {
                        using var document = JsonDocument.Parse(error);

                        var detail = document.RootElement
                            .GetProperty("detail")
                            .GetString();

                        if (detail == "Email already exists.")
                        {
                            throw new InvalidOperationException(
                                "Użytkownik z podanym adresem e-mail już istnieje.");
                        }
                    }
                    catch (JsonException)
                    {
                        // Odpowiedź API nie jest poprawnym JSON.
                    }
                    catch (KeyNotFoundException)
                    {
                        // Odpowiedź API nie zawiera pola detail.
                    }
                }

                throw new HttpRequestException(
                    $"API error {(int)response.StatusCode}",
                    null,
                    response.StatusCode);
            }

    }

}
