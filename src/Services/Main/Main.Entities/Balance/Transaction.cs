using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Exceptions;
using Main.Enums;
using Main.Enums.Balances;

namespace Main.Entities.Balance;

public class Transaction : AuditableEntity<Transaction, Guid>, ILinqEntity<Transaction, Guid>
{
    private Transaction()
    {
    }

    private Transaction(
        Guid senderId,
        Guid receiverId,
        int currencyId,
        TransactionType type,
        decimal transactionSum,
        DateTime transactionDatetime,
        TransactionSourceType sourceType)
    {
        if (senderId == receiverId)
            throw new InvalidInputException("transaction.sender.receiver.must.not.be.same");
        SenderId = senderId;
        ReceiverId = receiverId;
        Type = type;
        Status = TransactionStatus.Pending;
        SourceType = sourceType;
        SetTransactionDatetime(transactionDatetime);
        SetCurrencyId(currencyId);
        SetAmount(transactionSum);
    }

    [Validate]
    public Guid Id { get; private set; }

    public int CurrencyId { get; private set; }
    public Guid SenderId { get; }
    public Guid ReceiverId { get; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTime TransactionDatetime { get; private set; }
    public DateTime? ReversedAt { get; private set; }
    public Guid? ReversedBy { get; private set; }
    public TransactionSourceType SourceType { get; private set; }
    public uint RowVersion { get; private set; }

    public bool IsCompleted => Status.HasFlag(TransactionStatus.Completed);
    public bool IsCompletionApplied => Status.HasFlag(TransactionStatus.CompletionApplied);

    public bool IsReversed => Status.HasFlag(TransactionStatus.Reversed);
    public bool IsReversalApplied => Status.HasFlag(TransactionStatus.ReversedApplied);
    public User.User Receiver { get; private set; } = null!;
    public User.User Sender { get; private set; } = null!;

    public static Expression<Func<Transaction, Guid>> GetKeySelector()
    {
        return x => x.Id;
    }

    public static Expression<Func<Transaction, bool>> GetEqualityExpression(Guid key)
    {
        return x => x.Id == key;
    }

    public static Transaction Create(
        Guid senderId,
        Guid receiverId,
        int currencyId,
        TransactionType type,
        decimal transactionSum,
        DateTime transactionDatetime,
        TransactionSourceType sourceType)
    {
        return new Transaction(
            senderId, 
            receiverId, 
            currencyId, 
            type, 
            transactionSum, 
            transactionDatetime,
            sourceType);
    }

    private void SetAmount(decimal newAmount)
    {
        Amount = newAmount.AgainstTooManyDecimalPlaces(2, "transaction.amount.max.two.decimal.places")
            .AgainstLessOrEqual(0m, "transaction.amount.must.be.positive");
    }

    private void SetCurrencyId(int currencyId)
    {
        currencyId.AgainstLessOrEqual(
            0,
            () => new ArgumentException("CurrencyId must be greater than zero"));
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
        EnsureCanApply();

        if (IsReversed)
        {
            ApplyReversed(senderBalance, receiverBalance);
            Status |= TransactionStatus.ReversedApplied;
            return;
        }

        if (IsCompleted)
        {
            ApplyCompleted(senderBalance, receiverBalance);
            Status |= TransactionStatus.CompletionApplied;
        }
    }

    public void Apply(
        UserBalance senderBalance,
        UserBalance receiverBalance,
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        Guid systemId)
    {
        ValidateBalances(senderBalance, receiverBalance);
        ValidateFinancialProfiles(senderProfile, receiverProfile);
        EnsureCanApply();

        ApplyFinancialProfiles(
            senderProfile,
            receiverProfile,
            amountInBaseCurrency,
            systemId);
        Apply(senderBalance, receiverBalance);
    }

    private void ApplyFinancialProfiles(
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        Guid systemId)
    {
        if (SourceType == TransactionSourceType.Manual)
        {
            ApplyManualFinancialProfiles(senderProfile, receiverProfile, amountInBaseCurrency, systemId);
            return;
        }

        ApplySystemSettlementFinancialProfiles(senderProfile, receiverProfile, amountInBaseCurrency, systemId);
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

    private void ValidateFinancialProfiles(
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile)
    {
        if (senderProfile.UserId != SenderId)
            throw new InvalidOperationException("Sender financial profile user mismatch");
        if (receiverProfile.UserId != ReceiverId)
            throw new InvalidOperationException("Receiver financial profile user mismatch");
    }

    private void EnsureCanApply()
    {
        if (!IsCompleted && !IsReversed)
            throw new InvalidOperationException("Nothing to apply");

        if (IsReversed)
        {
            if (!IsCompletionApplied)
                throw new InvalidOperationException("Cannot reverse before completion is applied");
            if (IsReversalApplied)
                throw new InvalidOperationException("Reversed already applied.");
            return;
        }

        if (IsCompleted && IsCompletionApplied)
            throw new InvalidOperationException("Completion already applied.");
    }

    private void ApplyManualFinancialProfiles(
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        Guid systemId)
    {
        var senderIsSystem = SenderId == systemId;
        var receiverIsSystem = ReceiverId == systemId;

        if (IsReversed)
        {
            ApplyManualReversedFinancialProfiles(
                senderProfile,
                receiverProfile,
                amountInBaseCurrency,
                senderIsSystem,
                receiverIsSystem);
            return;
        }

        ApplyManualCompletedFinancialProfiles(
            senderProfile,
            receiverProfile,
            amountInBaseCurrency,
            senderIsSystem,
            receiverIsSystem);
    }

    private static void ApplyManualCompletedFinancialProfiles(
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        bool senderIsSystem,
        bool receiverIsSystem)
    {
        switch (senderIsSystem)
        {
            case true when !receiverIsSystem:
                receiverProfile.ReceiveFromSystem(amountInBaseCurrency);
                return;
            case false when receiverIsSystem:
                senderProfile.PayToSystem(amountInBaseCurrency);
                return;
            default:
                senderProfile.SpendAvailable(amountInBaseCurrency);
                receiverProfile.DepositWallet(amountInBaseCurrency);
                return;
        }
    }

    private static void ApplyManualReversedFinancialProfiles(
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        bool senderIsSystem,
        bool receiverIsSystem)
    {
        switch (senderIsSystem)
        {
            case true when !receiverIsSystem:
                receiverProfile.PayToSystem(amountInBaseCurrency);
                return;
            case false when receiverIsSystem:
                senderProfile.ReceiveFromSystem(amountInBaseCurrency);
                return;
            default:
                senderProfile.DepositWallet(amountInBaseCurrency);
                receiverProfile.SpendAvailable(amountInBaseCurrency);
                return;
        }
    }

    private void ApplySystemSettlementFinancialProfiles(
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        Guid systemId)
    {
        var senderIsSystem = SenderId == systemId;
        var receiverIsSystem = ReceiverId == systemId;

        if (IsReversed)
        {
            ApplySystemSettlementReversedFinancialProfiles(
                senderProfile,
                receiverProfile,
                amountInBaseCurrency,
                senderIsSystem,
                receiverIsSystem);
            return;
        }

        ApplySystemSettlementCompletedFinancialProfiles(
            senderProfile,
            receiverProfile,
            amountInBaseCurrency,
            senderIsSystem,
            receiverIsSystem);
    }

    private static void ApplySystemSettlementCompletedFinancialProfiles(
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        bool senderIsSystem,
        bool receiverIsSystem)
    {
        switch (senderIsSystem)
        {
            case false when receiverIsSystem:
                senderProfile.IncreaseSystemBalance(amountInBaseCurrency);
                return;
            case true when !receiverIsSystem:
                receiverProfile.DecreaseSystemBalance(amountInBaseCurrency);
                return;
        }
    }

    private static void ApplySystemSettlementReversedFinancialProfiles(
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        bool senderIsSystem,
        bool receiverIsSystem)
    {
        switch (senderIsSystem)
        {
            case false when receiverIsSystem:
                senderProfile.DecreaseSystemBalance(amountInBaseCurrency);
                return;
            case true when !receiverIsSystem:
                receiverProfile.IncreaseSystemBalance(amountInBaseCurrency);
                return;
        }
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

    public override Guid GetId()
    {
        return Id;
    }
}
