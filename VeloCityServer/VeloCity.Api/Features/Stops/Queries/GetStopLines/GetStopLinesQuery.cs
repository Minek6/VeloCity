using MediatR;
using VeloCity.Api.Features.Stops.DTOs;

namespace VeloCity.Api.Features.Stops.Queries.GetStopLines;

public record GetStopLinesQuery(int StopId)
    : IRequest<List<StopLineDto>>;
