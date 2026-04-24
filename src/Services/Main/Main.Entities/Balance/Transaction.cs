using System.Collections.Immutable;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Main.Entities.Exceptions.Balances;
using Main.Enums;

namespace Main.Entities.Transaction;

public class Transaction : AuditableEntity<Transaction, Guid>
{
    [Validate]
    public Guid Id { get; private set; }
    public int CurrencyId { get; private set; }
    public Guid SenderId { get; private set; }
    public Guid ReceiverId { get; private set; }
    public decimal TransactionSum { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTime TransactionDatetime { get; private set; }

    public decimal ReceiverBalanceAfterTransaction { get; private set; }
    public decimal SenderBalanceAfterTransaction { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }
    
    public uint RowVersion { get; private set; }
    
    private Transaction() {}

    private Transaction(
        Guid senderId, 
        Guid receiverId, 
        int currencyId, 
        TransactionStatus status, 
        decimal transactionSum, 
        Transaction? prevSenderTransaction,
        Transaction? prevReceiverTransaction,
        DateTime transactionDatetime)
    {
        SenderId = senderId;
        ReceiverId = receiverId;
        CurrencyId = currencyId;
        Status = status;
        TransactionDatetime = transactionDatetime;
        SetPrevBalances(prevSenderTransaction, prevReceiverTransaction);
        SetTransactionSum(transactionSum);
    }

    public static Transaction Create(
        Guid senderId,
        Guid receiverId,
        int currencyId,
        TransactionStatus status,
        decimal transactionSum,
        Transaction? prevSenderTransaction,
        Transaction? prevReceiverTransaction,
        DateTime transactionDatetime)
    {
        return new Transaction(senderId, receiverId, currencyId, status, transactionSum, prevSenderTransaction,
            prevReceiverTransaction, transactionDatetime);
    }

    public static Transaction CopyFrom(Transaction source)
    {
        return new Transaction
        {
            CurrencyId = source.CurrencyId,
            DeletedAt = source.DeletedAt,
            DeletedBy = source.DeletedBy,
            Id = source.Id,
            IsDeleted = source.IsDeleted,
            ReceiverBalanceAfterTransaction = source.ReceiverBalanceAfterTransaction,
            ReceiverId = source.ReceiverId,
            RowVersion = source.RowVersion,
            SenderId = source.SenderId,
            SenderBalanceAfterTransaction = source.SenderBalanceAfterTransaction,
            Status = source.Status,
            TransactionDatetime = source.TransactionDatetime,
            TransactionSum = source.TransactionSum,
        };
    }
    
    private static readonly ImmutableHashSet<TransactionStatus> AllowedToDeleteStatuses =
    [
        TransactionStatus.Normal
    ];

    public void Delete(Guid deletedBy, bool isSystem)
    {
        IsDeleted.AgainstEqual(
            next: true,
            () => new TransactionAlreadyDeletedException(Id));

        if (!isSystem && !AllowedToDeleteStatuses.Contains(Status))
            throw new BadTransactionStatusException(Status.ToString());
        
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void SetPrevBalances(Transaction? prevSenderTransaction, Transaction? prevReceiverTransaction)
    {
        var prevSenderBalance = ExtractPrevBalance(SenderId, prevSenderTransaction);
        var prevReceiverBalance = ExtractPrevBalance(ReceiverId, prevReceiverTransaction);
        
        ReceiverBalanceAfterTransaction = prevReceiverBalance + TransactionSum;
        SenderBalanceAfterTransaction = prevSenderBalance - TransactionSum;
    }
    
    public void SetTransactionSum(decimal newAmount)
    {
        newAmount.AgainstTooManyDecimalPlaces(2, "transaction.amount.max.two.decimal.places")
            .AgainstLessOrEqual(0m, "transaction.amount.must.be.positive");
        
        ReceiverBalanceAfterTransaction = (ReceiverBalanceAfterTransaction - TransactionSum + newAmount)
            .AgainstLessOrEqual(
                min: 0m,
                exceptionFactory: () => new InvalidOperationException(
                    "ReceiverBalanceAfterTransaction after calculation is less or equal to 0"));
        
        SenderBalanceAfterTransaction = (SenderBalanceAfterTransaction + TransactionSum - newAmount)
            .AgainstLessOrEqual(
                min: 0m,
                exceptionFactory: () => new InvalidOperationException(
                    "SenderBalanceAfterTransaction after calculation is less or equal to 0"));
        
        TransactionSum = newAmount;
    }

    public void SetCurrencyId(int currencyId)
    {
        CurrencyId = currencyId;
    }

    public void SetTransactionDatetime(DateTime newDatetime)
    {
        TransactionDatetime = newDatetime;
    }

    private static decimal ExtractPrevBalance(Guid userId, Transaction? prevTransaction)
    {
        if (prevTransaction == null) return 0m;
        return prevTransaction.SenderId == userId 
            ? prevTransaction.SenderBalanceAfterTransaction 
            : prevTransaction.ReceiverBalanceAfterTransaction;
    }
    public override Guid GetId() => Id;
}