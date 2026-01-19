using Main.Entities;

namespace Main.Abstractions.Interfaces.Services;

public interface IBalanceService
{
    Task ChangeSenderReceiverBalancesAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task RecalculateBalanceAsync(Transaction transaction, Guid? withOut = null,
        CancellationToken cancellationToken = default);
}