using Enums;

namespace Main.Modules.Balance.Abstractions;

public interface IFinanceTransfers
{
	Guid SystemOrganizationId { get; }

	Task<Guid> TransferAsync(
		Guid senderId,
		Guid receiverId,
		decimal amount,
		int currencyId,
		DateTime transactionDateTime,
		TransactionSourceType sourceType,
		bool forcePayment = false,
		CancellationToken cancellationToken = default);

	Task ReverseAsync(Guid transactionId, CancellationToken cancellationToken = default);
}
