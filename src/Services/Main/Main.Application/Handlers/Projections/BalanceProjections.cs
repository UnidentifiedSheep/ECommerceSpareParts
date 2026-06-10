using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Balances;
using Main.Application.Extensions;
using Main.Entities.Balance;
using Main.Entities.User;
using Main.Enums;
using Main.Enums.Balances;

namespace Main.Application.Handlers.Projections;

public static class BalanceProjections
{
    private static readonly string SystemRole = Role.System.ToNormalizedRole();
    
    public static readonly Expression<Func<Transaction, TransactionDto>> ToTransactionDto =
        x => new TransactionDto
        {
            Amount = x.Amount,
            Id = x.Id,
            CurrencyId = x.CurrencyId,
            Receiver = new TransactionPartyDto
            {
                PartyType = UserProjections.TransactionPartyTypeProjection.Invoke(x.Receiver),
                User = x.Receiver.Roles.Any(role => role.RoleName == SystemRole)
                    ? null
                    : UserProjections.UserProjection.Invoke(x.Receiver)
            },
            Sender = new TransactionPartyDto
            {
                PartyType = UserProjections.TransactionPartyTypeProjection.Invoke(x.Sender),
                User = x.Sender.Roles.Any(role => role.RoleName == SystemRole)
                    ? null
                    : UserProjections.UserProjection.Invoke(x.Sender)
            },
            Status = x.Status,
            Type = x.Type,
            TransactionDate = x.TransactionDatetime,
            SourceType = x.SourceType
        };
}
