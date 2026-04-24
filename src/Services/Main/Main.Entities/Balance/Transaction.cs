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
        SetPrevBalances(
            ExtractPrevBalance(senderId, prevSenderTransaction), 
            ExtractPrevBalance(receiverId, prevReceiverTransaction));
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

    public void Delete(Guid deletedBy)
    {
        IsDeleted.AgainstEqual(
            next: true,
            () => new TransactionAlreadyDeletedException(Id));
        
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void SetPrevBalances(decimal prevReceiverBalance, decimal prevSenderBalance)
    {
        prevReceiverBalance
            .AgainstTooManyDecimalPlaces(
                maxDecimals: 2, 
                exceptionFactory: () => new InvalidOperationException("Prev balance must have maximum 2 decimal places"))
            .AgainstLessOrEqual(
                min: 0m, 
                exceptionFactory: () => new InvalidOperationException("Prev balance must be greater than 0"));
        
        prevSenderBalance
            .AgainstTooManyDecimalPlaces(
                maxDecimals: 2, 
                exceptionFactory: () => new InvalidOperationException("Prev balance must have maximum 2 decimal places"))
            .AgainstLessOrEqual(
                min: 0m, 
                exceptionFactory: () => new InvalidOperationException("Prev balance must be greater than 0"));
        
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

    private static decimal ExtractPrevBalance(Guid userId, Transaction? prevTransaction)
    {
        if (prevTransaction == null) return 0m;
        return prevTransaction.SenderId == userId 
            ? prevTransaction.SenderBalanceAfterTransaction 
            : prevTransaction.ReceiverBalanceAfterTransaction;
    }
    public override Guid GetId() => Id;
}