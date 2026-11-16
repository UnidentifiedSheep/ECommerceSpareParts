using Abstractions.Models.Options;
using Enums;
using Main.Entities.Balance.Enums.Balances;
using Main.Modules.Balance.Abstractions;
using Main.Modules.Balance.Application.Handlers.Balance.CreateTransaction;
using Main.Modules.Balance.Application.Handlers.Balance.ReverseTransaction;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Modules.Balance.Application.Services;

public sealed class FinanceTransfers(ISender sender, IOptions<SystemOptions> systemOptions) : IFinanceTransfers
{
	public Guid SystemOrganizationId => systemOptions.Value.SystemId;

	public async Task<Guid> TransferAsync(
		Guid senderId,
		Guid receiverId,
		decimal amount,
		int currencyId,
		DateTime transactionDateTime,
		TransactionSourceType sourceType,
		bool forcePayment = false,
		CancellationToken cancellationToken = default)
		=> (await sender.Send(
				new CreateTransactionCommand(
					senderId,
					receiverId,
					amount,
					currencyId,
					transactionDateTime,
					sourceType,
					TransactionCreationMode.System,
					forcePayment),
				cancellationToken))
			.Transaction.Id;

	public async Task ReverseAsync(Guid transactionId, CancellationToken cancellationToken = default)
		=> await sender.Send(
			new ReverseTransactionCommand(
				transactionId,
				TransactionReversalMode.System,
				true),
			cancellationToken);
}
