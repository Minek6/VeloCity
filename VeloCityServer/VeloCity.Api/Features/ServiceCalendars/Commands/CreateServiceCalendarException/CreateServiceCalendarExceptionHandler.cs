using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Models.Data;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Models;

namespace VeloCity.Api.Features.ServiceCalendars.Commands.CreateServiceCalendarException;

public class CreateServiceCalendarExceptionHandler(ApplicationDbContext context)
    : IRequestHandler<CreateServiceCalendarExceptionCommand, int>
{
    public async Task<int> Handle(
        CreateServiceCalendarExceptionCommand request,
        CancellationToken cancellationToken)
    {
        var calendarExists = await context.ServiceCalendars
            .AnyAsync(
                c => c.Id == request.ServiceCalendarId,
                cancellationToken);

        if (!calendarExists)
        {
            throw new NotFoundException(
                "ServiceCalendar",
                request.ServiceCalendarId);
        }

        var exceptionExists = await context.ServiceCalendarExceptions
            .AnyAsync(
                e => e.ServiceCalendarId == request.ServiceCalendarId
                     && e.Date == request.Date,
                cancellationToken);

        if (exceptionExists)
        {
            throw new ConflictException(
                "An exception already exists for this calendar and date.");
        }

        var calendarException = new ServiceCalendarException
        {
            ServiceCalendarId = request.ServiceCalendarId,
            Date = request.Date,
            IsAdded = request.IsAdded
        };

        context.ServiceCalendarExceptions.Add(calendarException);

        await context.SaveChangesAsync(cancellationToken);

        return calendarException.Id;
    }
}
