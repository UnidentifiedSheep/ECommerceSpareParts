using System.Collections.Immutable;
using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Balances;
using Exceptions.Exceptions.Users;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Entities;
using Main.Core.Enums;
using Main.Core.Interfaces.DbRepositories;
using Main.Core.Interfaces.Services;
using MediatR;

namespace Main.Application.Handlers.Balance.DeleteTransaction;

[Transactional(IsolationLevel.Serializable, 20, 3)]
public record DeleteTransactionCommand(Guid TransactionId, Guid WhoDeleteUserId, bool IsSystem = false)
    : ICommand<Unit>;

public class DeleteTransactionHandler(
    IBalanceRepository balanceRepository,
    DbDataValidatorBase dbValidator,
    IUnitOfWork unitOfWork,
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
        await EnsureDataIsValid(transaction, whoDelete, request.IsSystem, cancellationToken);

        transaction.IsDeleted = true;
        transaction.DeletedAt = DateTime.Now;
        transaction.DeletedBy = whoDelete;

        await balanceService.ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);
        await balanceService.RecalculateBalanceAsync(transaction, transaction.Id, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task EnsureDataIsValid(Transaction transaction, Guid whoDeletedUserId, bool isSystem,
        CancellationToken ct = default)
    {
        if (transaction.IsDeleted)
            throw new TransactionAlreadyDeletedException(transaction.Id);
        if (!isSystem && !AllowedStatuses.Contains(transaction.Status))
            throw new BadTransactionStatusException(transaction.Status.ToString());
        
        var plan = new ValidationPlan().EnsureUserExists(whoDeletedUserId);
        await dbValidator.Validate(plan, true, true, ct);
    }
}