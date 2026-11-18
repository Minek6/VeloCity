using MediatR;

namespace VeloCity.Api.Features.Users.Commands.AdjustUserBalance;

public record AdjustUserBalanceCommand(
    int UserId,
    decimal Amount,
    string Reason
) : IRequest<BalanceAdjustmentDto>;

public record BalanceAdjustmentDto(
    int Id,
    int UserId,
    decimal Amount,
    decimal BalanceBefore,
    decimal BalanceAfter,
    string Reason,
    DateTime CreatedAt
);
