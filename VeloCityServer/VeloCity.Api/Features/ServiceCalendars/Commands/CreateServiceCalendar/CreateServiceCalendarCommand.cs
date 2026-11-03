using MediatR;

namespace VeloCity.Api.Features.ServiceCalendars.Commands.CreateServiceCalendar;

public record CreateServiceCalendarCommand(
    string Name,
    DateOnly ValidFrom,
    DateOnly ValidTo,
    bool Monday,
    bool Tuesday,
    bool Wednesday,
    bool Thursday,
    bool Friday,
    bool Saturday,
    bool Sunday
) : IRequest<int>;
