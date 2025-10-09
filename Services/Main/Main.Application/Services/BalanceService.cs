using Core.Entities;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;

namespace Main.Application.Services;

public class BalanceService(IBalanceRepository balanceRepository, IUnitOfWork unitOfWork) : IBalanceService
{
    public async Task ChangeSenderReceiverBalancesAsync(Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        var senderBalance = await balanceRepository.GetUserBalanceAsync(transaction.SenderId, transaction.CurrencyId,
            true, cancellationToken);
        var receiverBalance = await balanceRepository.GetUserBalanceAsync(transaction.ReceiverId,
            transaction.CurrencyId,
            true, cancellationToken);
        if (senderBalance == null)
        {
            senderBalance = new UserBalance
            {
                CurrencyId = transaction.CurrencyId,
                UserId = transaction.SenderId,
                Balance = 0
            };
            await unitOfWork.AddAsync(senderBalance, cancellationToken);
        }

        if (receiverBalance == null)
        {
            receiverBalance = new UserBalance
            {
                CurrencyId = transaction.CurrencyId,
                UserId = transaction.ReceiverId,
                Balance = 0
            };
            await unitOfWork.AddAsync(receiverBalance, cancellationToken);
        }

        var multiplier = transaction.IsDeleted ? -1 : 1;
        receiverBalance.Balance += multiplier * transaction.TransactionSum;
        senderBalance.Balance -= multiplier * transaction.TransactionSum;
    }

    public async Task RecalculateBalanceAsync(Transaction transaction, string? withOut = null,
        CancellationToken cancellationToken = default)
    {
        await foreach (var tr in balanceRepository.GetAffectedTransactions(transaction.ReceiverId,
                               transaction.CurrencyId, transaction.TransactionDatetime, withOut)
                           .WithCancellation(cancellationToken))
        {
            var amountDelta = transaction.IsDeleted ? -transaction.TransactionSum : transaction.TransactionSum;

            if (tr.SenderId == transaction.ReceiverId) tr.SenderBalanceAfterTransaction += amountDelta;
            else tr.ReceiverBalanceAfterTransaction += amountDelta;
        }

        await foreach (var tr in balanceRepository.GetAffectedTransactions(transaction.SenderId, transaction.CurrencyId,
                               transaction.TransactionDatetime, withOut)
                           .WithCancellation(cancellationToken))
        {
            var amountDelta = transaction.IsDeleted ? transaction.TransactionSum : -transaction.TransactionSum;

            if (tr.SenderId == transaction.SenderId) tr.SenderBalanceAfterTransaction += amountDelta;
            else tr.ReceiverBalanceAfterTransaction += amountDelta;
        }
    }
}