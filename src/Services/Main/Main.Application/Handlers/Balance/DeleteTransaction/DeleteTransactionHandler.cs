using System.Collections.Immutable;
using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Balances;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;
using Main.Entities;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.Balance.DeleteTransaction;

[Transactional(IsolationLevel.Serializable, 20, 3)]
public record DeleteTransactionCommand(Guid TransactionId, Guid WhoDeleteUserId, bool IsSystem = false)
    : ICommand<Unit>;

public class DeleteTransactionHandler(IBalanceRepository balanceRepository, IUnitOfWork unitOfWork,
    IBalanceService balanceService) : ICommandHandler<DeleteTransactionCommand, Unit>
{
    private static readonly ImmutableHashSet<TransactionStatus> AllowedStatuses =
    [
        TransactionStatus.Normal
    ];

    public async Task<Unit> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transactionId = request.TransactionId;
        var whoDelete = request.WhoDeleteUserId;
        var transaction = await balanceRepository.GetTransactionByIdAsync(transactionId, true, cancellationToken)
                          ?? throw new TransactionNotFoundExcpetion(transactionId);
        CheckTransaction(transaction, request.IsSystem);

        transaction.IsDeleted = true;
        transaction.DeletedAt = DateTime.Now;
        transaction.DeletedBy = whoDelete;

        await balanceService.ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);
        await balanceService.RecalculateBalanceAsync(transaction, transaction.Id, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private void CheckTransaction(Transaction transaction, bool isSystem)
    {
        if (transaction.IsDeleted)
            throw new TransactionAlreadyDeletedException(transaction.Id);
        if (!isSystem && !AllowedStatuses.Contains(transaction.Status))
            throw new BadTransactionStatusException(transaction.Status.ToString());
    }
}