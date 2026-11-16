
namespace Main.Entities.Balance.Balance;

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
