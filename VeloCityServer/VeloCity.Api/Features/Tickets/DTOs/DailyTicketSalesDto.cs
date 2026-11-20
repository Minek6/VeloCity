namespace VeloCity.Api.Features.Dashboard.DTOs;

public record DailyTicketSalesDto(
    DateOnly Date,
    int TicketsSold,
    decimal TotalAmount);
