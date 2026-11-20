using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeloCity.Api.Features.Dashboard.Queries.GetTicketSalesChart;
using VeloCity.Api.Features.Dashboard.Queries.GetDashboardStats;
using VeloCity.Api.Features.Dashboard.Queries.GetTicketSalesReport;
using VeloCity.Api.Features.Dashboard.Queries.GetTicketTypeSalesRanking;
using VeloCity.Api.Features.Dashboard.Queries.GetSystemHealth;
using VeloCity.Api.Features.Dashboard.DTOs;

namespace VeloCity.Api.Controllers;

[ApiController]
[Route("[controller]")]
//[Authorize(Roles = nameof(UserRole.Admin))]
public class DashboardController(IMediator mediator)
    : ControllerBase
{
    
    [HttpGet("ticket-sales")]
    [ProducesResponseType(
        typeof(List<DailyTicketSalesDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTicketSales(
        [FromQuery] int days = 7)
    {
        var result = await mediator.Send(
            new GetTicketSalesChartQuery(days));

        return Ok(result);
    }

    [HttpGet("stats")]
    [ProducesResponseType(
        typeof(DashboardStatsDto),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await mediator.Send(
            new GetDashboardStatsQuery());

        return Ok(result);
    }

    [HttpGet("ticket-sales/report")]
    [ProducesResponseType(
        typeof(List<DailyTicketSalesDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTicketSalesReport(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int? ticketTypeId,
        [FromQuery] bool? isValidated)
    {
        var query = new GetTicketSalesReportQuery(
            from,
            to,
            ticketTypeId,
            isValidated);

        var result = await mediator.Send(query);

        return Ok(result);
    }

    [HttpGet("ticket-sales/ranking")]
    [ProducesResponseType(
        typeof(List<TicketTypeSalesDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTicketTypeSalesRanking(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int? ticketTypeId,
        [FromQuery] bool? isValidated)
    {
        var result = await mediator.Send(
            new GetTicketTypeSalesRankingQuery(
                from,
                to,
                ticketTypeId,
                isValidated));

        return Ok(result);
    }

    [HttpGet("system-health")]
    [ProducesResponseType(
        typeof(SystemHealthDto),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemHealth()
    {
        var result = await mediator.Send(
            new GetSystemHealthQuery());

        return Ok(result);
    }

}