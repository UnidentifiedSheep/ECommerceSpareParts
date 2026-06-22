using Main.Entities.Balance;

namespace Main.Application.Interfaces.Services;

public interface IBalanceService
{
    Task ChangeSenderReceiverBalancesAsync(
        Transaction transaction,
        bool forceFinancialProfileDebit = false,
        CancellationToken cancellationToken = default);
}
