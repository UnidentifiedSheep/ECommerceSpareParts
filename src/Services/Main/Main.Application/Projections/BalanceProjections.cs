using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Balances;
using Main.Entities.Balance;

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

    public static readonly Expression<Func<UserBalance, UserBalanceDto>> ToUserBalanceDto =
        x => new UserBalanceDto
        {
            Balance = x.Balance,
            Currency = CurrencyProjections.ToDto.Invoke(x.Currency)
        };

    public static readonly Expression<Func<UserFinancialProfile, UserFinancialProfileDto>>
        ToUserFinancialProfileDto = x => new UserFinancialProfileDto
        {
            Balance = x.Balance,
            MinimalAllowedBalance = x.MinAllowedBalance
        };
}