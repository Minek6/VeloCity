namespace VeloCity.Api.Features.Tickets.DTOs;

public record AdminTicketDto(
    int Id,
    int UserId,
    string UserName,
    string UserSurname,
    string TicketTypeName,
    decimal Price,
    DateTime PurchasedAt,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    int? VehicleId,
    bool IsValidated);
