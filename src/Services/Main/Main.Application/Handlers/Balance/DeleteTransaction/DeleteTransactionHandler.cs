using System.Collections.Immutable;
using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Interfaces.Persistence;
using Main.Entities;
using Main.Entities.Exceptions.Balances;
using Main.Entities.Transaction;
using Main.Enums;

namespace Main.Application.Handlers.Balance.DeleteTransaction;

[Transactional(IsolationLevel.Serializable, 20, 3)]
public record DeleteTransactionCommand(Guid TransactionId, Guid WhoDeleteUserId, bool IsSystem = false)
    : ICommand<DeleteTransactionResult>;

public record DeleteTransactionResult(Transaction Transaction);

public class DeleteTransactionHandler(
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork,
    IBalanceService balanceService) : ICommandHandler<DeleteTransactionCommand, DeleteTransactionResult>
{
    

    public async Task<DeleteTransactionResult> Handle(
        DeleteTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var transactionId = request.TransactionId;
        var whoDelete = request.WhoDeleteUserId;
        var criteria = Criteria<Transaction>.New()
            .Where(x => x.Id == transactionId)
            .ForUpdate()
            .Track()
            .Build();
        
        var transaction = await transactionRepository.FirstOrDefaultAsync(criteria, cancellationToken)
                          ?? throw new TransactionNotFoundException(transactionId);

        transaction.Delete(request.WhoDeleteUserId, request.IsSystem);

        await balanceService.ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);
        await balanceService.RecalculateBalanceAsync(transaction, transaction.Id, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new DeleteTransactionResult(transaction);
    }
}