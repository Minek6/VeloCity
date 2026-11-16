
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace VeloCity.Api.Common.Services.Qr;

public sealed class TicketQrService
{
    private readonly byte[] _secret;

    public TicketQrService(IConfiguration configuration)
    {
        var encodedSecret = configuration["TicketQr:Secret"]
            ?? throw new InvalidOperationException(
                "TicketQr:Secret is not configured.");

        _secret = Convert.FromBase64String(encodedSecret);

        if (_secret.Length < 32)
        {
            throw new InvalidOperationException(
                "Ticket QR secret must contain at least 32 bytes.");
        }
    }

    public string Generate(int ticketId)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(60)
            .ToUnixTimeSeconds();

        var payload = JsonSerializer.SerializeToUtf8Bytes(new
        {
            ticketId,
            expiresAt
        });

        var encodedPayload = WebEncoders.Base64UrlEncode(payload);

        var signedData = $"v1.{encodedPayload}";

        var signature = HMACSHA256.HashData(
            _secret,
            Encoding.UTF8.GetBytes(signedData));

        return $"{signedData}.{WebEncoders.Base64UrlEncode(signature)}";
    }

    public bool TryVerify(string token, out int ticketId)
    {
        ticketId = 0;

        if (string.IsNullOrWhiteSpace(token) || token.Length > 4096)
            return false;

        try
        {
            var parts = token.Split('.');

            if (parts.Length != 3 || parts[0] != "v1")
                return false;

            var signedData = $"{parts[0]}.{parts[1]}";

            var expected = HMACSHA256.HashData(
                _secret,
                Encoding.UTF8.GetBytes(signedData));

            var supplied = WebEncoders.Base64UrlDecode(parts[2]);

            if (supplied.Length != expected.Length ||
                !CryptographicOperations.FixedTimeEquals(
                    expected, supplied))
                return false;

            var bytes = WebEncoders.Base64UrlDecode(parts[1]);

            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;

            var id = root.GetProperty("ticketId").GetInt32();
            var expiresAt = root.GetProperty("expiresAt").GetInt64();

            if (id <= 0 ||
                DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= expiresAt)
                return false;

            ticketId = id;
            return true;
        }
        catch (Exception ex) when (
            ex is FormatException ||
            ex is JsonException ||
            ex is InvalidOperationException ||
            ex is OverflowException ||
            ex is ArgumentException)
        {
            return false;
        }
    }
}
