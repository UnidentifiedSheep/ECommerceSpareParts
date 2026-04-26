using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Exceptions;
using Main.Entities.User;
using Main.Enums;

namespace Main.Entities.Balance;

public class Transaction : AuditableEntity<Transaction, Guid>
{
    [Validate]
    public Guid Id { get; private set; }
    public int CurrencyId { get; private set; }
    public Guid SenderId { get; private set; }
    public Guid ReceiverId { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTime TransactionDatetime { get; private set; }
    public DateTime? ReversedAt { get; private set; }
    public Guid? ReversedBy { get; private set; }
    public uint RowVersion { get; private set; }
    
    public bool IsCompleted => Status.HasFlag(TransactionStatus.Completed);
    public bool IsCompletionApplied => Status.HasFlag(TransactionStatus.CompletionApplied);

    public bool IsReversed => Status.HasFlag(TransactionStatus.Reversed);
    public bool IsReversalApplied => Status.HasFlag(TransactionStatus.ReversedApplied);
    
    private Transaction() {}

    private Transaction(
        Guid senderId, 
        Guid receiverId, 
        int currencyId, 
        TransactionType type, 
        decimal transactionSum,
        DateTime transactionDatetime)
    {
        if (senderId == receiverId)
            throw new InvalidInputException(""); //TODO: create error message.
        SenderId = senderId;
        ReceiverId = receiverId;
        Type = type;
        Status = TransactionStatus.Pending;
        SetTransactionDatetime(transactionDatetime);
        SetCurrencyId(currencyId);
        SetAmount(transactionSum);
    }

    public static Transaction Create(
        Guid senderId,
        Guid receiverId,
        int currencyId,
        TransactionType type,
        decimal transactionSum,
        DateTime transactionDatetime)
    {
        return new Transaction(senderId, receiverId, currencyId, type, transactionSum, transactionDatetime);
    }
    
    private void SetAmount(decimal newAmount)
    {
        Amount = newAmount.AgainstTooManyDecimalPlaces(2, "transaction.amount.max.two.decimal.places")
            .AgainstLessOrEqual(0m, "transaction.amount.must.be.positive");
    }

    private void SetCurrencyId(int currencyId)
    {
        currencyId.AgainstLessOrEqual(
            min: 0,
            exceptionFactory: () => new ArgumentException("CurrencyId must be greater than zero"));
        CurrencyId = currencyId;
    }

    private void SetTransactionDatetime(DateTime newDatetime)
    {
        TransactionDatetime = newDatetime;
    }

    public void Complete()
    {
        EnsureCanMutate();

        if (Status != TransactionStatus.Pending)
            throw new InvalidOperationException("Only Pending transactions can be completed");

        Status |= TransactionStatus.Completed;
    }

    public void Reverse(Guid reversedBy)
    {
        EnsureCanMutate();

        if (!IsCompleted)
            throw new InvalidOperationException("Only Completed transactions can be reversed");

        if (!IsCompletionApplied)
            throw new InvalidOperationException("Cannot reverse before completion is applied");

        Status |= TransactionStatus.Reversed;
        ReversedAt = DateTime.UtcNow;
        ReversedBy = reversedBy;
    }

    public void Apply(UserBalance senderBalance, UserBalance receiverBalance)
    {
        ValidateBalances(senderBalance, receiverBalance);
        
        if (!IsCompleted && !IsReversed)
            throw new InvalidOperationException("Nothing to apply");
        
        if (IsReversed)
        {
            if (!IsCompletionApplied)
                throw new InvalidOperationException("Cannot reverse before completion is applied");
            if (IsReversalApplied)
                throw new InvalidOperationException("Reversed already applied.");
            ApplyReversed(senderBalance, receiverBalance);
            Status |= TransactionStatus.ReversedApplied;
            return;
        }
        
        if (IsCompleted)
        {
            if (IsCompletionApplied)
                throw new InvalidOperationException("Completion already applied.");
            ApplyCompleted(senderBalance, receiverBalance);
            Status |= TransactionStatus.CompletionApplied;
        }
    }

    private void ValidateBalances(UserBalance senderBalance, UserBalance receiverBalance)
    {
        if (senderBalance.CurrencyId != CurrencyId)
            throw new InvalidOperationException("Sender balance currency mismatch");
        if (receiverBalance.CurrencyId != CurrencyId)
            throw new InvalidOperationException("Receiver balance currency mismatch");
        if (senderBalance.UserId != SenderId)
            throw new InvalidOperationException("Sender balance user mismatch");
        if (receiverBalance.UserId != ReceiverId)
            throw new InvalidOperationException("Receiver balance user mismatch");
    }

    private void ApplyCompleted(UserBalance sender, UserBalance receiver)
    {
        sender.IncrementBalance(-Amount);
        receiver.IncrementBalance(Amount);
    }

    private void ApplyReversed(UserBalance sender, UserBalance receiver)
    {
        sender.IncrementBalance(Amount);
        receiver.IncrementBalance(-Amount);
    }
    
    private void EnsureCanMutate()
    {
        if (IsCompleted && IsReversed)
            throw new InvalidOperationException("Invalid state: both Completed and Reversed");

        if (IsReversed)
            throw new InvalidOperationException("Transaction is in terminal state");
        
    }

    public override Guid GetId() => Id;
}