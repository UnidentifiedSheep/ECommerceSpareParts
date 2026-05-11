using Main.Entities.Balance;

namespace Main.Application.Interfaces.Services;

public interface IBalanceService
{
    Task ChangeSenderReceiverBalancesAsync(Transaction transaction, CancellationToken cancellationToken = default);
}