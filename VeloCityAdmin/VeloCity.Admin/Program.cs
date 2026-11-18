using VeloCity.Admin.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using VeloCity.Admin.Models;
using VeloCity.Admin.Authentication;
using Microsoft.Extensions.Caching.Memory;
using VeloCity.Admin.Services;
using VeloCity.Admin.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient("VeloCityApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException(
            "Api:BaseUrl is not configured."));
});


builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "VeloCity.Admin.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;

        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";

        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin"));
});

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITicketStore, MemoryTicketStore>();

builder.Services
    .AddOptions<CookieAuthenticationOptions>(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure<ITicketStore>((options, store) =>
    {
        options.SessionStore = store;
    });

builder.Services.AddScoped<VehiclesApiService>();
builder.Services.AddScoped<VehiclesViewModel>();
builder.Services.AddScoped<AdminApiClientFactory>();
builder.Services.AddScoped<UsersApiService>();
builder.Services.AddScoped<UsersViewModel>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.MapPost("/auth/logout", async (
    HttpContext http,
    IAntiforgery antiforgery) =>
{
    try
    {
        await antiforgery.ValidateRequestAsync(http);
    }
    catch (AntiforgeryValidationException)
    {
        return Results.BadRequest("Invalid form token.");
    }

    var sessionId = http.User
        .FindFirst("AdminSessionId")?.Value;

    if (!string.IsNullOrEmpty(sessionId))
    {
        http.RequestServices
            .GetRequiredService<IMemoryCache>()
            .Remove($"admin-api-token:{sessionId}");
    }

    await http.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme);

    return Results.Redirect("/login");
})
.RequireAuthorization();

app.MapPost("/auth/login", async (
    HttpContext http,
    IHttpClientFactory httpClientFactory,
    IAntiforgery antiforgery) =>
{
    try
    {
        await antiforgery.ValidateRequestAsync(http);
    }
    catch (AntiforgeryValidationException)
    {
        return Results.BadRequest("Invalid form token.");
    }

    var form = await http.Request.ReadFormAsync();

    var email = form["email"].ToString().Trim();
    var password = form["password"].ToString();

    if (string.IsNullOrWhiteSpace(email) ||
        string.IsNullOrWhiteSpace(password))
    {
        return Results.Redirect("/login?error=1");
    }

    try
    {
        var client =
            httpClientFactory.CreateClient("VeloCityApi");

        using var response = await client.PostAsJsonAsync(
            "Users/login",
            new
            {
                Email = email,
                Password = password
            });

        if (!response.IsSuccessStatusCode)
        {
            return Results.Redirect("/login?error=1");
        }

        var login = await response.Content
            .ReadFromJsonAsync<LoginResponse>();

        if (login is null ||
            string.IsNullOrWhiteSpace(login.Token) ||
            login.Role != "Admin")
        {
            return Results.Redirect("/login?error=2");
        }

        var sessionId = Guid.NewGuid().ToString("N");

        var cache = http.RequestServices
            .GetRequiredService<IMemoryCache>();

        cache.Set(
            $"admin-api-token:{sessionId}",
            login.Token,
            TimeSpan.FromHours(2));

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, login.Name),
            new(ClaimTypes.Role, login.Role),
            new("AdminSessionId", sessionId)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        
        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
        };

        properties.StoreTokens(
        [
            new AuthenticationToken
            {
                Name = "access_token",
                Value = login.Token
            }
        ]);

        await http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);


        return Results.Redirect("/");
    }
    catch (HttpRequestException)
    {
        return Results.Redirect("/login?error=3");
    }
})
.AllowAnonymous();


app.MapGet("/admin-api/test", async (
    HttpContext http,
    IHttpClientFactory httpClientFactory) =>
{
    var token = await http.GetTokenAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        "access_token");

    if (string.IsNullOrWhiteSpace(token))
    {
        return Results.Unauthorized();
    }

    var client = httpClientFactory.CreateClient("VeloCityApi");

    using var request = new HttpRequestMessage(
        HttpMethod.Get,
        "Users/profile");

    request.Headers.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            token);

    using var response = await client.SendAsync(request);

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem(
            "Nie udało się pobrać profilu z VeloCity.Api.",
            statusCode: (int)response.StatusCode);
    }

    // Nie zwracamy tutaj pełnego profilu ani JWT.
    return Results.Ok(new
    {
        success = true,
        message = "VeloCity.Admin połączył się z VeloCity.Api."
    });
})
.RequireAuthorization("AdminOnly");


app.Run();
