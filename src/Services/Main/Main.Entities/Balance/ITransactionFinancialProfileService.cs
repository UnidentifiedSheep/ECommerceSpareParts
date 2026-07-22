using Main.Entities.Organization;

namespace Main.Entities.Balance;

public interface ITransactionFinancialProfileService
{
    void Apply(
        Transaction transaction,
        OrganizationFinancialProfile senderProfile,
        OrganizationFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        Guid systemId,
        bool forceDebit = false);
}