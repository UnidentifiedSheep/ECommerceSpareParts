using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Balances;
using Main.Entities.Balance;
using Main.Entities.Organization;

namespace Main.Application.Projections;

public static class BalanceProjections
{
    public static readonly Expression<Func<Transaction, TransactionDto>> ToTransactionDto =
        x => new TransactionDto
        {
            Amount = x.Amount,
            Id = x.Id,
            CurrencyId = x.CurrencyId,
            Receiver = UserProjections.UserPartyProjection.Invoke(x.Receiver),
            Sender = UserProjections.UserPartyProjection.Invoke(x.Sender),
            Status = x.Status,
            Type = x.Type,
            TransactionDate = x.TransactionDatetime,
            SourceType = x.SourceType
        };

    public static readonly Expression<Func<OrganizationBalance, OrganizationBalanceDto>> ToOrganizationBalanceDto =
        x => new OrganizationBalanceDto
        {
            Balance = x.Balance,
            Currency = CurrencyProjections.ToDto.Invoke(x.Currency)
        };

    public static OrganizationFinancialProfileDto ToOrganizationFinancialProfileDto(
        OrganizationFinancialProfile profile,
        decimal netPositionInBaseCurrency)
    {
        return new OrganizationFinancialProfileDto
        {
            NetPositionInBaseCurrency = netPositionInBaseCurrency,
            MinimalAllowedBalance = profile.MinAllowedBalance
        };
    }
}
