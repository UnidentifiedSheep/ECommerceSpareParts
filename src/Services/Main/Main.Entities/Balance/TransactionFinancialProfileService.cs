using Main.Entities.Organization;
using Exceptions;

namespace Main.Entities.Balance;

public class TransactionFinancialProfileService : ITransactionFinancialProfileService
{
    public void Apply(
        Transaction transaction,
        OrganizationFinancialProfile senderProfile,
        OrganizationFinancialProfile receiverProfile,
        decimal senderBalanceInBaseCurrency,
        decimal receiverBalanceInBaseCurrency,
        decimal amountInBaseCurrency,
        bool forceDebit = false)
    {
        transaction.EnsureCanApplyProfile(senderProfile, receiverProfile);

        if (transaction.IsReversalApplied)
        {
            transaction.MarkReversalProfileApplied();
            return;
        }

        EnsureBalanceIsAllowed(
            senderProfile,
            senderBalanceInBaseCurrency,
            amountInBaseCurrency,
            forceDebit);
        EnsureBalanceIsAllowed(
            receiverProfile,
            receiverBalanceInBaseCurrency,
            -amountInBaseCurrency,
            forceDebit);

        transaction.MarkCompletionProfileApplied();
    }

    private static void EnsureBalanceIsAllowed(
        OrganizationFinancialProfile profile,
        decimal currentBalance,
        decimal delta,
        bool forceDebit)
    {
        if (forceDebit || delta >= 0 || currentBalance + delta >= profile.MinAllowedBalance)
            return;

        throw new InvalidInputException("financial.profile.balance.below.minimum");
    }
}
