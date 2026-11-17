using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace VeloCity.Admin.Authentication;

public sealed class MemoryTicketStore(
    IMemoryCache cache) : ITicketStore
{
    public Task<string> StoreAsync(
        AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString("N");

        SaveTicket(key, ticket);

        return Task.FromResult(key);
    }

    public Task RenewAsync(
        string key,
        AuthenticationTicket ticket)
    {
        SaveTicket(key, ticket);
        return Task.CompletedTask;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(
        string key)
    {
        cache.TryGetValue(
            key,
            out AuthenticationTicket? ticket);

        return Task.FromResult(ticket);
    }

    public Task RemoveAsync(string key)
    {
        cache.Remove(key);
        return Task.CompletedTask;
    }

    private void SaveTicket(
        string key,
        AuthenticationTicket ticket)
    {
        var expiration = ticket.Properties.ExpiresUtc
            ?? DateTimeOffset.UtcNow.AddHours(2);

        cache.Set(key, ticket, expiration);
    }
}
