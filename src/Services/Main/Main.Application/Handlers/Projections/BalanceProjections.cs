using System.Linq.Expressions;
using Main.Application.Dtos.Amw.Balances;
using Main.Entities.Balance;

namespace Main.Application.Handlers.Projections;

public static class BalanceProjections
{
    public static Expression<Func<Transaction, TransactionDto>> ToTransactionDto =
        x => new TransactionDto
        {
            Amount = x.Amount,
            Id = x.Id,
            CurrencyId = x.CurrencyId,
            ReceiverId = x.ReceiverId,
            SenderId = x.SenderId,
            Status = x.Status,
            Type = x.Type,
            TransactionDate = x.TransactionDatetime,
        };
}