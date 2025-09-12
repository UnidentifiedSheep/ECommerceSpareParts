using Application.Interfaces;
using Core.Dtos.Amw.Balances;
using Core.Interfaces.DbRepositories;
using Mapster;

namespace Application.Handlers.Balance.GetTransactions;

public record GetTransactionsQuery(DateTime RangeStart, DateTime RangeEnd, 
    int? CurrencyId, string? SenderId, string? ReceiverId, int Page, int ViewCount) : IQuery<GetTransactionsResult>;
public record GetTransactionsResult(IEnumerable<TransactionDto> Transactions);

public class GetTransactionsHandler(IBalanceRepository balanceRepository) : IQueryHandler<GetTransactionsQuery, GetTransactionsResult>
{
    public async Task<GetTransactionsResult> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var res = await balanceRepository.GetTransactionsAsync(request.RangeStart, request.RangeEnd, 
            request.CurrencyId, request.SenderId, request.ReceiverId, request.Page, request.ViewCount, false, cancellationToken);
        return new GetTransactionsResult(res.Adapt<List<TransactionDto>>());
    }
}