
namespace VeloCity.Api.Features.Tickets.DTOs;

public record TicketDto(
    int Id,
    string TicketTypeName,
    decimal Price,
    DateTime PurchasedAt,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    int? VehicleId,
    bool IsValidated,

    // Dane wybranego połączenia
    int? ScheduledTripId = null,
    int? ExternalRunId = null,
    DateOnly? ServiceDate = null,
    int? StartStopId = null,
    string? StartStopName = null,
    int? EndStopId = null,
    string? EndStopName = null,
    string? LineNumber = null,
    TimeSpan? DepartureTime = null,
    TimeSpan? ArrivalTime = null
);
