using System.Data;
using Abstractions.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Balance;
using Main.Entities.Exceptions;
using Main.Enums.Balances;

namespace Main.Application.Handlers.Balance.ReverseTransaction;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 3)]
public record ReverseTransactionCommand(
    Guid TransactionId,
    TransactionReversalMode Mode = TransactionReversalMode.User,
    bool ForcePayment = false)
    : ICommand<ReverseTransactionResult>;

public record ReverseTransactionResult(Transaction Transaction);

public class ReverseTransactionHandler(
    ITransactionRepository transactionRepository,
    IUserContext userContext,
    IBalanceService balanceService) : ICommandHandler<ReverseTransactionCommand, ReverseTransactionResult>
{
    public async Task<ReverseTransactionResult> Handle(
        ReverseTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var transactionId = request.TransactionId;
        var criteria = Criteria<Transaction>.New()
            .Where(x => x.Id == transactionId)
            .ForUpdate()
            .Track()
            .Build();

        var transaction = await transactionRepository.FirstOrDefaultAsync(criteria, cancellationToken)
                          ?? throw new TransactionNotFoundException(transactionId);

        if (request.Mode == TransactionReversalMode.User &&
            transaction.SourceType != TransactionSourceType.Manual)
            throw new TransactionSourceCannotBeReversedByUserException(transaction.SourceType);

        transaction.Reverse(userContext.UserId);
        await balanceService.ChangeSenderReceiverBalancesAsync(
            transaction,
            request.ForcePayment,
            cancellationToken: cancellationToken);
        return new ReverseTransactionResult(transaction);
    }
}
