using Main.Enums.Balances;

namespace Main.Entities.Balance;

public class TransactionFinancialProfileService : ITransactionFinancialProfileService
{
    public void Apply(
        Transaction transaction,
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        Guid systemId,
        bool forceDebit = false)
    {
        transaction.EnsureCanApplyProfile(senderProfile, receiverProfile);

        if (transaction.IsReversalApplied)
        {
            ApplyReversal(
                transaction,
                senderProfile,
                receiverProfile,
                amountInBaseCurrency,
                systemId,
                forceDebit);
            transaction.MarkReversalProfileApplied();
            return;
        }

        ApplyCompletion(
            transaction,
            senderProfile,
            receiverProfile,
            amountInBaseCurrency,
            systemId,
            forceDebit);
        transaction.MarkCompletionProfileApplied();
    }

    private static void ApplyCompletion(
        Transaction transaction,
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        Guid systemId,
        bool forceDebit)
    {
        switch (transaction.SourceType)
        {
            case TransactionSourceType.Purchase:
            case TransactionSourceType.Logistic:
                senderProfile.Credit(amountInBaseCurrency);
                break;
            case TransactionSourceType.Sale:
                receiverProfile.Debit(amountInBaseCurrency, forceDebit);
                break;
            case TransactionSourceType.Manual when transaction.ReceiverId == systemId:
                senderProfile.Credit(amountInBaseCurrency);
                break;
            case TransactionSourceType.Manual when transaction.SenderId == systemId:
                receiverProfile.Debit(amountInBaseCurrency, forceDebit);
                break;
            case TransactionSourceType.Manual:
                senderProfile.Debit(amountInBaseCurrency, forceDebit);
                receiverProfile.Credit(amountInBaseCurrency);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported transaction source type: {transaction.SourceType}");
        }
    }

    private static void ApplyReversal(
        Transaction transaction,
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        Guid systemId,
        bool forceDebit)
    {
        switch (transaction.SourceType)
        {
            case TransactionSourceType.Purchase:
            case TransactionSourceType.Logistic:
                senderProfile.Debit(amountInBaseCurrency, forceDebit);
                break;
            case TransactionSourceType.Sale:
                receiverProfile.Credit(amountInBaseCurrency);
                break;
            case TransactionSourceType.Manual when transaction.ReceiverId == systemId:
                senderProfile.Debit(amountInBaseCurrency, forceDebit);
                break;
            case TransactionSourceType.Manual when transaction.SenderId == systemId:
                receiverProfile.Credit(amountInBaseCurrency);
                break;
            case TransactionSourceType.Manual:
                senderProfile.Credit(amountInBaseCurrency);
                receiverProfile.Debit(amountInBaseCurrency, forceDebit);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported transaction source type: {transaction.SourceType}");
        }
    }
}
