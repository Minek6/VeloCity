namespace VeloCity.Api.Features.Dashboard.DTOs;

public record SystemHealthDto(
    bool DatabaseConnected,
    long DatabaseResponseTimeMs,
    DateTime CheckedAtUtc);
