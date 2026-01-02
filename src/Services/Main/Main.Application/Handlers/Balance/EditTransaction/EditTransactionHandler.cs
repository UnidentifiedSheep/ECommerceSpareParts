using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Balances;
using Main.Core.Entities;
using Main.Core.Enums;
using Main.Core.Interfaces.DbRepositories;
using Main.Core.Interfaces.Services;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Balance.EditTransaction;

[Transactional(IsolationLevel.Serializable, 20, 3)]
public record EditTransactionCommand(
    Guid TransactionId,
    int CurrencyId,
    decimal Amount,
    TransactionStatus Status,
    DateTime TransactionDateTime) : ICommand;

public class EditTransactionHandler(
    IBalanceRepository balanceRepository,
    IBalanceService balanceService,
    IUnitOfWork unitOfWork) : ICommandHandler<EditTransactionCommand>
{
    public async Task<Unit> Handle(EditTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await GetAndValidateTransactionAsync(request, cancellationToken);
        await CreateTransactionVersionAsync(transaction, cancellationToken);
        await RollbackTransactionAsync(transaction, cancellationToken);

        ApplyNewParameters(transaction, request);

        await RecalculateBalancesAsync(transaction, request, cancellationToken);
        await ApplyNewTransactionVersionAsync(transaction, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task<Transaction> GetAndValidateTransactionAsync(EditTransactionCommand request, CancellationToken ct)
    {
        var transaction = await balanceRepository.GetTransactionByIdAsync(request.TransactionId, true, ct)
                          ?? throw new TransactionNotFoundExcpetion(request.TransactionId);

        return transaction.IsDeleted ? throw new EditingDeletedTransactionException(request.TransactionId) : transaction;
    }

    private async Task CreateTransactionVersionAsync(Transaction transaction, CancellationToken ct)
    {
        var lastVersion = await balanceRepository.GetLastTransactionVersionAsync(transaction.Id, false, ct);
        var transactionVersion = transaction.Adapt<TransactionVersion>();
        transactionVersion.Version = lastVersion?.Version + 1 ?? 0;

        await unitOfWork.AddAsync(transactionVersion, ct);
    }

    private async Task RollbackTransactionAsync(Transaction transaction, CancellationToken ct)
    {
        transaction.IsDeleted = true;
        await balanceService.RecalculateBalanceAsync(transaction, transaction.Id, ct);
        await balanceService.ChangeSenderReceiverBalancesAsync(transaction, ct);
    }

    private void ApplyNewParameters(Transaction transaction, EditTransactionCommand request)
    {
        transaction.IsDeleted = false;
        transaction.TransactionDatetime = request.TransactionDateTime;
        transaction.TransactionSum = request.Amount;
        transaction.Status = request.Status.ToString();
        transaction.CurrencyId = request.CurrencyId;
    }

    private async Task RecalculateBalancesAsync(Transaction transaction, EditTransactionCommand request,
        CancellationToken ct)
    {
        var prevSenderTransaction = await balanceRepository
            .GetPreviousTransactionAsync(transaction.TransactionDatetime, transaction.SenderId, request.CurrencyId,
                true, ct);
        var prevReceiverTransaction = await balanceRepository
            .GetPreviousTransactionAsync(transaction.TransactionDatetime, transaction.ReceiverId, request.CurrencyId,
                true, ct);

        transaction.SenderBalanceAfterTransaction = prevSenderTransaction?.SenderId == transaction.SenderId
            ? prevSenderTransaction.SenderBalanceAfterTransaction - request.Amount
            : (prevSenderTransaction?.ReceiverBalanceAfterTransaction ?? 0) - request.Amount;

        transaction.ReceiverBalanceAfterTransaction = prevReceiverTransaction?.ReceiverId == transaction.ReceiverId
            ? prevReceiverTransaction.ReceiverBalanceAfterTransaction + request.Amount
            : (prevReceiverTransaction?.SenderBalanceAfterTransaction ?? 0) + request.Amount;
    }

    private async Task ApplyNewTransactionVersionAsync(Transaction transaction, CancellationToken ct)
    {
        await balanceService.RecalculateBalanceAsync(transaction, transaction.Id, ct);
        await balanceService.ChangeSenderReceiverBalancesAsync(transaction, ct);
    }
}