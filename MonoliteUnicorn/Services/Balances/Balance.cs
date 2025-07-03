using System.Collections.Immutable;
using Core.TransactionBuilder;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions.Balances;
using MonoliteUnicorn.Exceptions.Currencies;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.Extensions;
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

    private const string GetPrevTransaction = """
                                              Select * 
                                              from transactions
                                              where transaction_datetime < '{0}' and
                                              (sender_id = '{1}' or receiver_id = '{1}') and
                                              currency_id = {2}
                                              order by transaction_datetime desc
                                              for update
                                              limit 1
                                              """;

    public async Task<Transaction> CreateTransactionAsync(string senderId, string receiverId, decimal amount, TransactionStatus status, 
        int currencyId, string whoCreatedTransaction, DateTime transactionDateTime, CancellationToken cancellationToken = default)
    {
        amount = Math.Round(amount, 2);
        if (amount <= 0) throw new ZeroOrNegativeTransactionAmountException();
        if (senderId == receiverId) throw new SameSenderAndReceiverException();
        var sameTransactionExists = await context.Transactions.AsNoTracking()
            .AnyAsync(x => x.SenderId == senderId &&
                           x.ReceiverId == receiverId &&
                           x.TransactionDatetime == transactionDateTime, cancellationToken);
        if (sameTransactionExists) throw new SameTransactionExists();
        await context.EnsureCurrencyExists(currencyId, cancellationToken);
        await context.EnsureUserExists([senderId, receiverId, whoCreatedTransaction], cancellationToken);
        
        return await context.WithDefaultTransactionSettings("normal-with-isolation")
            .ExecuteWithTransaction(async () => 
            {
                transactionDateTime = DateTime.SpecifyKind(transactionDateTime, DateTimeKind.Unspecified);
                
                var prevSenderTransaction = await GetPreviousTransactionForUpdate(transactionDateTime, senderId, currencyId, cancellationToken);
                var prevReceiverTransaction = await GetPreviousTransactionForUpdate(transactionDateTime, receiverId, currencyId, cancellationToken);
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
                await ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);
                await RecalculateBalanceAsync(transaction, null, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                return transaction; 
            }, cancellationToken);
    }

    private async Task RecalculateBalanceAsync(Transaction transaction, string? withOut = null, CancellationToken cancellationToken = default)
    {
        await foreach (var tr in GetAffectedTransactions(transaction.ReceiverId, transaction.CurrencyId, transaction.TransactionDatetime, withOut)
                           .WithCancellation(cancellationToken))
        {
            var amountDelta = transaction.IsDeleted ? -transaction.TransactionSum : transaction.TransactionSum;

            if (tr.SenderId == transaction.ReceiverId) tr.SenderBalanceAfterTransaction += amountDelta;
            else tr.ReceiverBalanceAfterTransaction += amountDelta;
            
        }
        
        await foreach (var tr in GetAffectedTransactions(transaction.SenderId, transaction.CurrencyId, transaction.TransactionDatetime, withOut)
                           .WithCancellation(cancellationToken))
        {
            var amountDelta = transaction.IsDeleted ? transaction.TransactionSum : -transaction.TransactionSum;

            if (tr.SenderId == transaction.SenderId) tr.SenderBalanceAfterTransaction += amountDelta;
            else tr.ReceiverBalanceAfterTransaction += amountDelta;
            
        }
    }
    
    private IAsyncEnumerable<Transaction> GetAffectedTransactions(string userId, int currencyId, DateTime datetime, string? withOut = null)
    {
        var additional = string.IsNullOrWhiteSpace(withOut) ? "" : $"AND id != '{withOut}'";
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
        await context.EnsureUserExists(userId, cancellationToken);
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
        await context.EnsureUserExists(whoDeleteUserId, cancellationToken);
        await context.WithDefaultTransactionSettings("normal-with-isolation")
            .ExecuteWithTransaction(async () =>
            {
                var transactionEntity = await context.Transactions
                    .FromSql($"SELECT * FROM transactions WHERE id = {transactionId} FOR UPDATE")
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new TransactionNotFound(transactionId);
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
        await context.WithDefaultTransactionSettings("normal-with-isolation")
            .ExecuteWithTransaction(async () =>
            {
                amount = Math.Round(amount, 2);
                transactionDateTime = DateTime.SpecifyKind(transactionDateTime, DateTimeKind.Unspecified);

                var transaction = await context.Transactions
                    .FromSqlInterpolated($"""
                        SELECT * FROM transactions 
                        WHERE id = {transactionId} 
                        FOR UPDATE
                    """)
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new TransactionNotFound(transactionId);

                if (amount <= 0) throw new ZeroOrNegativeTransactionAmountException();
                if (transaction.IsDeleted) throw new EditingDeletedTransactionException(transactionId);

                var sameTransactionExists = await context.Transactions.AsNoTracking()
                    .AnyAsync(x => x.SenderId == transaction.SenderId &&
                                   x.ReceiverId == transaction.ReceiverId &&
                                   x.TransactionDatetime == transactionDateTime && x.Id != transactionId, cancellationToken);
                if (sameTransactionExists) throw new SameTransactionExists();
                
                // Создаём версию до изменений
                var lastVersion = await context.TransactionVersions
                    .AsNoTracking()
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync(x => x.TransactionId == transactionId, cancellationToken);

                var transactionVersion = transaction.Adapt<TransactionVersion>();
                transactionVersion.Version = lastVersion?.Version + 1 ?? 0;
                await context.AddAsync(transactionVersion, cancellationToken);

                // Откат текущей транзакции
                transaction.IsDeleted = true;
                await RecalculateBalanceAsync(transaction, transaction.Id, cancellationToken);
                await ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);

                // Применение новых параметров
                transaction.IsDeleted = false;
                transaction.TransactionDatetime = transactionDateTime;
                transaction.TransactionSum = amount;
                transaction.Status = status.ToString();
                transaction.CurrencyId = currencyId;

                // Получение предыдущих транзакций
                var prevSenderTransaction = await GetPreviousTransactionForUpdate(transactionDateTime, transaction.SenderId, currencyId, cancellationToken);
                var prevReceiverTransaction = await GetPreviousTransactionForUpdate(transactionDateTime, transaction.ReceiverId, currencyId, cancellationToken);

                transaction.SenderBalanceAfterTransaction = prevSenderTransaction?.SenderId == transaction.SenderId
                    ? prevSenderTransaction.SenderBalanceAfterTransaction - amount
                    : (prevSenderTransaction?.ReceiverBalanceAfterTransaction ?? 0) - amount;

                transaction.ReceiverBalanceAfterTransaction = prevReceiverTransaction?.ReceiverId == transaction.ReceiverId
                    ? prevReceiverTransaction.ReceiverBalanceAfterTransaction + amount
                    : (prevReceiverTransaction?.SenderBalanceAfterTransaction ?? 0) + amount;

                // Применение новой версии транзакции
                await RecalculateBalanceAsync(transaction, transaction.Id, cancellationToken);
                await ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
   }

    private async Task<Transaction?> GetPreviousTransactionForUpdate(DateTime transactionDateTime, string userId, int currencyId, CancellationToken cancellationToken)
    {
        return await context.Transactions
            .FromSqlRaw(string.Format(GetPrevTransaction, transactionDateTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff"), userId, currencyId))
            .FirstOrDefaultAsync(cancellationToken);
    }

}