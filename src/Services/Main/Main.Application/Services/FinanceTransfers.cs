using Abstractions.Models.Options;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Balance.ReverseTransaction;
using Main.Application.Interfaces.Services;
using Main.Modules.Balance.Abstractions;
using Main.Enums.Balances;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Application.Services;

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
