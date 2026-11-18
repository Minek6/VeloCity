using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Common.Interfaces;
using VeloCity.Api.Models;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Users.Commands.AdjustUserBalance;

public sealed class AdjustUserBalanceHandler(
    ApplicationDbContext context,
    IUserContext userContext)
    : IRequestHandler<
        AdjustUserBalanceCommand,
        BalanceAdjustmentDto>
{
    public async Task<BalanceAdjustmentDto> Handle(
        AdjustUserBalanceCommand request,
        CancellationToken ct)
    {
        if (!userContext.IsAdmin || userContext.Id is null)
        {
            throw new AppException(
                "Administrator authorization required.", 403);
        }

        var adminId = userContext.Id.Value;

        await using var transaction =
            await context.Database.BeginTransactionAsync(ct);

        // Atomowa zmiana salda z kontrolą limitu dolnego.
        var updatedRows = await context.Users
            .Where(u =>
                u.Id == request.UserId &&
                u.IsActive &&
                u.Balance + request.Amount >= 0m)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    u => u.Balance,
                    u => u.Balance + request.Amount),
                ct);

        if (updatedRows != 1)
        {
            var user = await context.Users
                .AsNoTracking()
                .Where(u => u.Id == request.UserId)
                .Select(u => new
                {
                    u.Id,
                    u.IsActive
                })
                .FirstOrDefaultAsync(ct);

            if (user is null)
            {
                throw new NotFoundException(
                    "User", request.UserId);
            }

            if (!user.IsActive)
            {
                throw new AppException(
                    "Cannot adjust balance of inactive user.", 400);
            }

            throw new AppException(
                "Insufficient balance for this adjustment.", 400);
        }

        // Pobieramy nowe saldo po wykonaniu aktualizacji.
        var balanceAfter = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => u.Balance)
            .SingleAsync(ct);

        var balanceBefore = balanceAfter - request.Amount;

        // Zapis historii operacji.
        var adjustment = new BalanceAdjustment
        {
            UserId = request.UserId,
            AdminId = adminId,
            Amount = request.Amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Reason = request.Reason.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        context.BalanceAdjustments.Add(adjustment);

        await context.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        return new BalanceAdjustmentDto(
            adjustment.Id,
            adjustment.UserId,
            adjustment.Amount,
            adjustment.BalanceBefore,
            adjustment.BalanceAfter,
            adjustment.Reason,
            adjustment.CreatedAt
        );
    }
}
