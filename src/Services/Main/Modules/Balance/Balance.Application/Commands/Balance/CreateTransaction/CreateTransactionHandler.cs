using System.Data;
using Abstractions.Models.Options;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Enums;
using Main.Entities.Balance.Balance;
using Main.Entities.Balance.Enums.Balances;
using Main.Entities.Balance.Exceptions;
using Main.Modules.Balance.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Main.Modules.Balance.Application.Commands.Balance.CreateTransaction;

[AutoSave]
[Transactional(
	IsolationLevel.ReadCommitted,
	20,
	3)]
public record CreateTransactionCommand(
	Guid SenderId,
	Guid ReceiverId,
	decimal Amount,
	int CurrencyId,
	DateTime TransactionDateTime,
	TransactionSourceType SourceType,
	TransactionCreationMode Mode = TransactionCreationMode.User,
	bool ForcePayment = false) : ICommand<CreateTransactionResult>;

public record CreateTransactionResult(Transaction Transaction);

public class CreateTransactionHandler(
	IBalanceService balanceService,
	IOptions<SystemOptions> systemOptions,
	IUnitOfWork unitOfWork) : ICommandHandler<CreateTransactionCommand, CreateTransactionResult>
{
	public async Task<CreateTransactionResult> Handle(
		CreateTransactionCommand request,
		CancellationToken cancellationToken)
	{
		if (request.Mode == TransactionCreationMode.User)
			WhenUserCreates(request);

		var transaction = Transaction.Create(
			request.SenderId,
			request.ReceiverId,
			request.CurrencyId,
			TransactionType.Transfer,
			request.Amount,
			request.TransactionDateTime,
			request.SourceType);

		transaction.Complete();
		await balanceService.ChangeSenderReceiverBalancesAsync(
			transaction,
			request.ForcePayment,
			cancellationToken);

		await unitOfWork.AddAsync(transaction, cancellationToken);
		return new CreateTransactionResult(transaction);
	}

	private void WhenUserCreates(CreateTransactionCommand command)
	{
		var systemOrganizationId = systemOptions.Value.SystemId;
		if (command.SenderId == systemOrganizationId || command.ReceiverId == systemOrganizationId)
			throw new TransactionWithSystemOrganizationCannotBeCreatedByUserException();
	}
}
