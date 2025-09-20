using System.Data;
using Application.Extensions;
using Application.Interfaces;
using Core.Attributes;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Balances;

namespace Application.Handlers.Balance.CreateTransaction;

[Transactional(IsolationLevel.Serializable, 20, 3)]
public record CreateTransactionCommand(
    Guid SenderId,
    Guid ReceiverId,
    decimal Amount,
    int CurrencyId,
    Guid WhoCreatedTransaction,
    DateTime TransactionDateTime,
    TransactionStatus TransactionStatus) : ICommand<CreateTransactionResult>;

public record CreateTransactionResult(Transaction Transaction);

public class CreateTransactionHandler(
    IBalanceRepository balanceRepository,
    ICurrencyRepository currencyRepository,
    IUserRepository usersRepository,
    IBalanceService balanceService,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateTransactionCommand, CreateTransactionResult>
{
    public async Task<CreateTransactionResult> Handle(CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var senderId = request.SenderId;
        var receiverId = request.ReceiverId;
        var whoCreatedTransaction = request.WhoCreatedTransaction;
        var amount = request.Amount;
        var currencyId = request.CurrencyId;
        var transactionDateTime = DateTime.SpecifyKind(request.TransactionDateTime, DateTimeKind.Unspecified);

        await EnsureNeededDataExists(senderId, receiverId, whoCreatedTransaction, transactionDateTime, currencyId,
            cancellationToken);

        var prevSenderTransaction = await balanceRepository.GetPreviousTransactionAsync(transactionDateTime, senderId,
            currencyId, true, cancellationToken);
        var prevReceiverTransaction = await balanceRepository.GetPreviousTransactionAsync(transactionDateTime,
            receiverId,
            currencyId, true, cancellationToken);

        var transaction = CreateTransaction(senderId, receiverId, currencyId, whoCreatedTransaction, amount,
            transactionDateTime,
            request.TransactionStatus, prevSenderTransaction, prevReceiverTransaction);

        await unitOfWork.AddAsync(transaction, cancellationToken);
        await balanceService.ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);
        await balanceService.RecalculateBalanceAsync(transaction, null, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateTransactionResult(transaction);
    }

    private async Task EnsureNeededDataExists(Guid senderId, Guid receiverId, Guid whoCreatedTransaction,
        DateTime transactionDateTime,
        int currencyId, CancellationToken cancellationToken = default)
    {
        if (await balanceRepository.TransactionExistsAsync(senderId, receiverId, transactionDateTime, null,
                cancellationToken))
            throw new SameTransactionExists();
        await currencyRepository.EnsureCurrenciesExists([currencyId], cancellationToken);
        await usersRepository.EnsureUsersExists([senderId, receiverId, whoCreatedTransaction], cancellationToken);
    }

    private Transaction CreateTransaction(Guid senderId, Guid receiverId, int currencyId,
        Guid whoCreatedTransaction,
        decimal amount, DateTime transactionDateTime, TransactionStatus transactionStatus,
        Transaction? prevSenderTransaction,
        Transaction? prevReceiverTransaction)
    {
        return new Transaction
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            CurrencyId = currencyId,
            WhoMadeUserId = whoCreatedTransaction,
            TransactionSum = amount,
            TransactionDatetime = transactionDateTime,
            ReceiverBalanceAfterTransaction = prevReceiverTransaction?.ReceiverId == receiverId
                ? prevReceiverTransaction.ReceiverBalanceAfterTransaction + amount
                : (prevReceiverTransaction?.SenderBalanceAfterTransaction ?? 0) + amount,
            SenderBalanceAfterTransaction = prevSenderTransaction?.SenderId == senderId
                ? prevSenderTransaction.SenderBalanceAfterTransaction - amount
                : (prevSenderTransaction?.ReceiverBalanceAfterTransaction ?? 0) - amount,
            Status = transactionStatus.ToString()
        };
    }
}