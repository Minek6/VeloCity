using MediatR;
using VeloCity.Api.Models;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.ServiceCalendars.Commands.CreateServiceCalendar;

public class CreateServiceCalendarHandler(ApplicationDbContext context)
    : IRequestHandler<CreateServiceCalendarCommand, int>
{
    public async Task<int> Handle(
        CreateServiceCalendarCommand request,
        CancellationToken ct)
    {
        var calendar = new ServiceCalendar
        {
            Name = request.Name.Trim(),
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            Monday = request.Monday,
            Tuesday = request.Tuesday,
            Wednesday = request.Wednesday,
            Thursday = request.Thursday,
            Friday = request.Friday,
            Saturday = request.Saturday,
            Sunday = request.Sunday
        };

        context.ServiceCalendars.Add(calendar);
        await context.SaveChangesAsync(ct);

        return calendar.Id;
    }
}
