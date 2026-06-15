using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Balances;
using Main.Application.Extensions;
using Main.Entities.Balance;
using Main.Enums;

namespace Main.Application.Projections;

public static class BalanceProjections
{
    private static readonly string SystemRole = Role.System.ToNormalizedRole();
    
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
}
