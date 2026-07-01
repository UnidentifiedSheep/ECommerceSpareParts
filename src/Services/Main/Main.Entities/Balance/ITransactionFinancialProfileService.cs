namespace Main.Entities.Balance;

public interface ITransactionFinancialProfileService
{
    void Apply(
        Transaction transaction,
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        Guid systemId,
        bool forceDebit = false);
}