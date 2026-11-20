namespace VeloCity.Api.Features.Dashboard.DTOs;

public record TicketTypeSalesDto(
    int TicketTypeId,
    string TicketTypeName,
    int TicketsSold,
    decimal TotalAmount,
    decimal Percentage);
