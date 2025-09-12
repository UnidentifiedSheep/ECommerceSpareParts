using System.Collections.Immutable;
using System.Data;
using Application.Extensions;
using Application.Interfaces;
using Core.Attributes;
using Core.Entities;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Balances;
using MediatR;
using TransactionStatus = Core.Enums.TransactionStatus;

namespace Application.Handlers.Balance.DeleteTransaction;

[Transactional(IsolationLevel.Serializable, 20, 3)]
public record DeleteTransactionCommand(string TransactionId, string WhoDeleteUserId, bool IsSystem = false) : ICommand<Unit>;


public class DeleteTransactionHandler(IBalanceRepository balanceRepository, IUsersRepository usersRepository, 
    IUnitOfWork unitOfWork, IBalanceService balanceService) : ICommandHandler<DeleteTransactionCommand, Unit>
{
    private static readonly ImmutableHashSet<string> AllowedStatuses =
    [
        nameof(TransactionStatus.Normal)
    ];
    
    public async Task<Unit> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transactionId = request.TransactionId;
        var whoDelete = request.WhoDeleteUserId;
        var transaction = await balanceRepository.GetTransactionByIdAsync(transactionId, true, cancellationToken)
                          ?? throw new TransactionDoesntExistsException(transactionId);
        await EnsureDataIsValid(transaction, whoDelete, request.IsSystem, cancellationToken);
        
        transaction.IsDeleted = true;
        transaction.DeletedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        transaction.DeletedBy = whoDelete;
            
        await balanceService.ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);
        await balanceService.RecalculateBalanceAsync(transaction, transaction.Id, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task EnsureDataIsValid(Transaction transaction, string whoDeletedUserId, bool isSystem, CancellationToken ct = default)
    {
        if (transaction.IsDeleted) 
            throw new TransactionAlreadyDeletedException(transaction.Id);
        if (!isSystem && !AllowedStatuses.Contains(transaction.Status)) 
            throw new BadTransactionStatusException(transaction.Status);
        await usersRepository.EnsureUsersExists([whoDeletedUserId], ct);
    }
}