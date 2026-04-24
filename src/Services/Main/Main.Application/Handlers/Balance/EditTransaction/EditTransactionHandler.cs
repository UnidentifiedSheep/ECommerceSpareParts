using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Event;
using Main.Entities.Exceptions.Balances;
using Main.Entities.Transaction;

namespace Main.Application.Handlers.Balance.EditTransaction;

[Transactional(IsolationLevel.Serializable, 20, 3)]
public record EditTransactionCommand(
    Guid TransactionId,
    int CurrencyId,
    decimal Amount,
    DateTime TransactionDateTime) : ICommand<EditTransactionResult>;

public record EditTransactionResult(Transaction Transaction);

public class EditTransactionHandler(
    ITransactionRepository transactionRepository,
    IBalanceService balanceService,
    IUnitOfWork unitOfWork) : ICommandHandler<EditTransactionCommand, EditTransactionResult>
{
    public async Task<EditTransactionResult> Handle(EditTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await GetAndValidateTransactionAsync(request, cancellationToken);
        await CreateTransactionVersionAsync(transaction, cancellationToken);
        await RollbackTransactionAsync(transaction, cancellationToken);
        
        transaction.SetTransactionSum(request.Amount);
        transaction.SetTransactionDatetime(request.TransactionDateTime);
        transaction.SetCurrencyId(request.CurrencyId);

        await RecalculateBalancesAsync(transaction, cancellationToken);
        await ApplyNewTransactionVersionAsync(transaction, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new EditTransactionResult(transaction);
    }

    private async Task<Transaction> GetAndValidateTransactionAsync(EditTransactionCommand request, CancellationToken ct)
    {
        var criteria = Criteria<Transaction>.New()
            .Where(x => x.Id == request.TransactionId)
            .ForUpdate()
            .Track()
            .Build();
        
        var transaction = await transactionRepository.FirstOrDefaultAsync(criteria, ct)
                          ?? throw new TransactionNotFoundException(request.TransactionId);

        return transaction.IsDeleted
            ? throw new EditingDeletedTransactionException(request.TransactionId)
            : transaction;
    }

    private async Task CreateTransactionVersionAsync(Transaction transaction, CancellationToken ct)
    {
        var version = TransactionUpdatedEvent.Create(transaction);
        await unitOfWork.AddAsync(version, ct);
    }

    private async Task RollbackTransactionAsync(Transaction transaction, CancellationToken ct)
    {
        Transaction copy = Transaction.CopyFrom(transaction);
        copy.Delete(Guid.Empty, true);
        
        await balanceService.RecalculateBalanceAsync(transaction, transaction.Id, ct);
        await balanceService.ChangeSenderReceiverBalancesAsync(transaction, ct);
    }

    private async Task RecalculateBalancesAsync(
        Transaction transaction,
        CancellationToken ct)
    {
        var criteria = Criteria<Transaction>.New()
            .ForUpdate()
            .Build();
        var prevSenderTransaction = await transactionRepository
            .GetPreviousTransactionAsync(transaction.TransactionDatetime, transaction.SenderId, transaction.CurrencyId,
                criteria, ct);
        var prevReceiverTransaction = await transactionRepository
            .GetPreviousTransactionAsync(transaction.TransactionDatetime, transaction.ReceiverId, transaction.CurrencyId,
                criteria, ct);

        transaction.SetPrevBalances(prevSenderTransaction, prevReceiverTransaction);
    }

    private async Task ApplyNewTransactionVersionAsync(Transaction transaction, CancellationToken ct)
    {
        await balanceService.RecalculateBalanceAsync(transaction, transaction.Id, ct);
        await balanceService.ChangeSenderReceiverBalancesAsync(transaction, ct);
    }
}