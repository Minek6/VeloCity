namespace VeloCity.Api.Features.Tickets.DTOs;

public record TicketValidityAdjustmentDto(
    int Id,
    int TicketId,
    int AdminId,
    string AdminName,
    string AdminSurname,
    DateTime PreviousValidTo,
    DateTime NewValidTo,
    string Reason,
    DateTime CreatedAt);
