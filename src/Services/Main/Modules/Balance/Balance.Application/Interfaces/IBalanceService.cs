using Main.Entities.Balance.Balance;

namespace Main.Modules.Balance.Application.Interfaces;

public interface IBalanceService
{
	Task<decimal> GetBalanceInBaseCurrencyAsync(
		Guid organizationId,
		CancellationToken cancellationToken = default);

	Task RecalculateApproximateBalancesAsync(
		IReadOnlyCollection<Guid> organizationIds,
		CancellationToken cancellationToken = default);

	Task ChangeSenderReceiverBalancesAsync(
		Transaction transaction,
		bool forceFinancialProfileDebit = false,
		CancellationToken cancellationToken = default);
}
