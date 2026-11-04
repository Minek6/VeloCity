using MediatR;

namespace VeloCity.Api.Features.ServiceCalendars.Commands.CreateServiceCalendarException;

public record CreateServiceCalendarExceptionCommand(
    int ServiceCalendarId,
    DateOnly Date,
    bool IsAdded
) : IRequest<int>;
