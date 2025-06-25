using System.Collections.Immutable;
using Core.Extensions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions.Balances;
using MonoliteUnicorn.Exceptions.Currencies;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.Services.Balances;

public class Balance(DContext context) : IBalance
{
    
    private static readonly ImmutableArray<string> AllowedStatuses =
    [
        nameof(TransactionStatus.Normal),
        nameof(TransactionStatus.Purchase),
        nameof(TransactionStatus.Sale)
    ];
    public async Task<Transaction> CreateTransactionAsync(string senderId, string receiverId, decimal amount, TransactionStatus status, 
        int currencyId, string whoCreatedTransaction, DateTime transactionDateTime, CancellationToken cancellationToken = default)
    {
        return await context.WithTransactionAsync(async () =>
        {
            transactionDateTime = DateTime.SpecifyKind(transactionDateTime, DateTimeKind.Unspecified);

            if (amount <= 0) throw new ZeroOrNegativeTransactionAmountException();
            if (senderId == receiverId) throw new SameSenderAndReceiverException();
            var sameTransactionExists = await context.Transactions.AsNoTracking()
                .AnyAsync(x => x.SenderId == senderId &&
                               x.ReceiverId == receiverId &&
                               x.TransactionDatetime == transactionDateTime, cancellationToken);
            if (sameTransactionExists) throw new SameTransactionExists();
            _ = await context.Currencies.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == currencyId, cancellationToken) ??
                throw new CurrencyNotFoundException(currencyId);
            _ = await context.AspNetUsers.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == senderId, cancellationToken) ??
                throw new UserNotFoundException(senderId);
            _ = await context.AspNetUsers.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == receiverId, cancellationToken) ??
                throw new UserNotFoundException(receiverId);
            _ = await context.AspNetUsers.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == whoCreatedTransaction, cancellationToken) ??
                throw new UserNotFoundException(whoCreatedTransaction);
            var prevSenderTransaction = await context.Transactions.AsNoTracking()
                .OrderByDescending(x => x.TransactionDatetime)
                .FirstOrDefaultAsync(x => x.TransactionDatetime < transactionDateTime &&
                                          (x.SenderId == senderId || x.ReceiverId == senderId), cancellationToken);
            var prevReceiverTransaction = await context.Transactions.AsNoTracking()
                .OrderByDescending(x => x.TransactionDatetime)
                .FirstOrDefaultAsync(x => x.TransactionDatetime < transactionDateTime &&
                                          (x.SenderId == receiverId || x.ReceiverId == receiverId), cancellationToken);
            var transaction = new Transaction
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                CurrencyId = currencyId,
                WhoMadeUserId = whoCreatedTransaction,
                TransactionSum = amount,
                TransactionDatetime = transactionDateTime,
                ReceiverBalanceAfterTransaction = prevReceiverTransaction?.ReceiverId == receiverId
                    ? (prevReceiverTransaction.ReceiverBalanceAfterTransaction) + amount
                    : (prevReceiverTransaction?.SenderBalanceAfterTransaction ?? 0) + amount,
                SenderBalanceAfterTransaction = prevSenderTransaction?.SenderId == senderId
                    ? (prevSenderTransaction.SenderBalanceAfterTransaction) - amount
                    : (prevSenderTransaction?.ReceiverBalanceAfterTransaction ?? 0) - amount,
                Status = status.ToString()
            };
            await context.Transactions.AddAsync(transaction, cancellationToken);
            await RecalculateBalanceAsync(transaction, null, cancellationToken);
            await ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return transaction;
        }, cancellationToken);
    }

    private async Task RecalculateBalanceAsync(Transaction transaction, string? withOut = null, CancellationToken cancellationToken = default)
    {
        await foreach (var tr in GetAffectedTransactions(transaction.ReceiverId, transaction.CurrencyId, transaction.TransactionDatetime)
                           .WithCancellation(cancellationToken))
        {
            var amountDelta = transaction.IsDeleted ? -transaction.TransactionSum : transaction.TransactionSum;

            if (tr.SenderId == transaction.ReceiverId) tr.SenderBalanceAfterTransaction += amountDelta;
            else tr.ReceiverBalanceAfterTransaction += amountDelta;
            
        }
        
        await foreach (var tr in GetAffectedTransactions(transaction.SenderId, transaction.CurrencyId, transaction.TransactionDatetime)
                           .WithCancellation(cancellationToken))
        {
            var amountDelta = transaction.IsDeleted ? transaction.TransactionSum : -transaction.TransactionSum;

            if (tr.SenderId == transaction.SenderId) tr.SenderBalanceAfterTransaction += amountDelta;
            else tr.ReceiverBalanceAfterTransaction += amountDelta;
            
        }
    }
    
    private IAsyncEnumerable<Transaction> GetAffectedTransactions(string userId, int currencyId, DateTime datetime, string? withOut = null)
    {
        var additional = string.IsNullOrWhiteSpace(withOut) ? "" : $"AND transaction_id != '{withOut}'";
        return context.Transactions
            .FromSqlRaw($"""
                             SELECT * FROM transactions 
                             WHERE currency_id = {currencyId} 
                             {additional}
                             AND transaction_datetime >= '{datetime:yyyy-MM-dd HH:mm:ss.ffffff}'
                             AND (sender_id = '{userId}' OR receiver_id = '{userId}')
                             AND is_deleted = false
                             ORDER BY transaction_datetime DESC, id DESC FOR UPDATE
                         """)
            .AsAsyncEnumerable();
    }

    private async Task ChangeSenderReceiverBalancesAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var senderBalance = await context.UserBalances
            .FromSql($"""
                      Select * from user_balances 
                               where user_id = {transaction.SenderId} and 
                                     currency_id = {transaction.CurrencyId}
                               for update
                      """)
            .FirstOrDefaultAsync(cancellationToken);
        var receiverBalance = await context.UserBalances
            .FromSql($"""
                      Select * from user_balances 
                                              where user_id = {transaction.ReceiverId} and 
                                                    currency_id = {transaction.CurrencyId}
                                              for update
                      """)
            .FirstOrDefaultAsync(cancellationToken);
        if (senderBalance == null)
        {
            senderBalance = new UserBalance
            {
                CurrencyId = transaction.CurrencyId,
                UserId = transaction.SenderId,
                Balance = 0,
            };
            await context.UserBalances.AddAsync(senderBalance, cancellationToken);
        }
        if (receiverBalance == null)
        {
            receiverBalance = new UserBalance
            {
                CurrencyId = transaction.CurrencyId,
                UserId = transaction.ReceiverId,
                Balance = 0,
            };
            await context.UserBalances.AddAsync(receiverBalance, cancellationToken);
        }

        var multiplier = transaction.IsDeleted ? -1 : 1;
        receiverBalance.Balance += multiplier * transaction.TransactionSum;
        senderBalance.Balance -= multiplier * transaction.TransactionSum;
    }
    
    public async Task ChangeUsersDiscount(string userId, decimal discount, CancellationToken cancellationToken = default)
    {
        var userFound = await context.AspNetUsers.AsNoTracking().AnyAsync(x => x.Id == userId, cancellationToken);
        if (!userFound) throw new UserNotFoundException(userId);
        if (discount < 0) throw new ArgumentOutOfRangeException(nameof(discount), "Скидка не может быть меньше 0");
        if (discount > 100) throw new ArgumentOutOfRangeException(nameof(discount), "Скидка не может быть больше 100");
        await context.Database.ExecuteSqlAsync($"""
                                             INSERT INTO user_discounts (user_id, discount)
                                             VALUES ({userId}, {discount})
                                             ON CONFLICT (user_id)
                                             DO UPDATE SET discount = EXCLUDED.discount;
                                             """, cancellationToken);
    }

    public async Task DeleteTransaction(string transactionId, string whoDeleteUserId, CancellationToken cancellationToken = default)
    {
        await context.WithTransactionAsync(async () =>
        {
            var transactionEntity = await context.Transactions
                .FromSql($"SELECT * FROM transactions WHERE id = {transactionId} FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken) ?? throw new TransactionNotFount(transactionId);
            var whoDeleted = await context.AspNetUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == whoDeleteUserId, cancellationToken);
            if (whoDeleted == null) throw new UserNotFoundException(whoDeleteUserId);
            if (transactionEntity.IsDeleted) throw new TransactionAlreadyDeletedException(transactionId);
            if (!AllowedStatuses.Contains(transactionEntity.Status)) 
                throw new BadTransactionStatusException(transactionEntity.Status);

            transactionEntity.IsDeleted = true;
            transactionEntity.DeletedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            transactionEntity.DeletedBy = whoDeleteUserId;
            
            await RecalculateBalanceAsync(transactionEntity, transactionEntity.Id, cancellationToken);
            await ChangeSenderReceiverBalancesAsync(transactionEntity, cancellationToken);
            
            await context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task EditTransaction(string transactionId, int currencyId, decimal amount, TransactionStatus status, DateTime transactionDateTime, CancellationToken cancellationToken = default)
    {
        await context.WithTransactionAsync(async () =>
        {
            var transaction = await context.Transactions
                .FromSql($"SELECT * FROM transactions WHERE id = {transactionId} for update")
                .FirstOrDefaultAsync(cancellationToken) ?? throw new TransactionNotFount(transactionId);
            if (amount <= 0) throw new ZeroOrNegativeTransactionAmountException();
            if (transaction.IsDeleted) throw new EditingDeletedTransactionException(transactionId);
            var lastVersion = await context.TransactionVersions
                .AsNoTracking()
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync(x => x.TransactionId == transactionId, cancellationToken);
            var transactionVersion = transaction.Adapt<TransactionVersion>();
            transactionVersion.Version = lastVersion == null ? 0 : lastVersion.Version + 1;
            await context.AddAsync(transactionVersion, cancellationToken);

            if (transaction.CurrencyId != currencyId)
            {
                transaction.IsDeleted = true;
                await RecalculateBalanceAsync(transaction, transaction.Id, cancellationToken);
                await ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);
                transaction.IsDeleted = false;
            }
            
            transaction.Status = status.ToString();
            transaction.TransactionDatetime = transactionDateTime;
            transaction.TransactionSum = amount;
            transaction.CurrencyId = currencyId;
        
            await RecalculateBalanceAsync(transaction, transaction.Id, cancellationToken);
            await ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);
        
            await context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }

}