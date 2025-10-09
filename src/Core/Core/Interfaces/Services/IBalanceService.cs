using Core.Entities;

namespace Core.Interfaces.Services;

public interface IBalanceService
{
    Task ChangeSenderReceiverBalancesAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task RecalculateBalanceAsync(Transaction transaction, string? withOut = null,
        CancellationToken cancellationToken = default);
}