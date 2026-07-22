using Main.Entities.Organization;

namespace Main.Entities.Balance;

public interface ITransactionFinancialProfileService
{
    void Apply(
        Transaction transaction,
        OrganizationFinancialProfile senderProfile,
        OrganizationFinancialProfile receiverProfile,
        decimal senderBalanceInBaseCurrency,
        decimal receiverBalanceInBaseCurrency,
        decimal amountInBaseCurrency,
        bool forceDebit = false);
}
