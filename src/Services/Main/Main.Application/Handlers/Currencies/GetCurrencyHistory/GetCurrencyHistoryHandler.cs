using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Currencies;
using Main.Entities.Currency;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Currencies.GetCurrencyHistory;

[Diagnostics(maxExecutionTimeMs: 200)]
public record GetCurrencyHistoryQuery(int CurrencyId, Pagination Pagination)
    : IQuery<GetCurrencyHistoryResult>;

public record GetCurrencyHistoryResult(IReadOnlyList<CurrencyRateHistoryDto> History);

public class GetCurrencyHistoryHandler(
    IReadRepository<CurrencyRateHistory, int> repository
)
    : IQueryHandler<GetCurrencyHistoryQuery, GetCurrencyHistoryResult>
{
    public async Task<GetCurrencyHistoryResult> Handle(
        GetCurrencyHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var history = await repository.Query
            .Where(x => x.FromCurrencyId == request.CurrencyId ||
                        x.ToCurrencyId == request.CurrencyId)
            .Select(x => new CurrencyRateHistoryDto
            {
                Id = x.Id,
                FromCurrencyId = x.FromCurrencyId,
                ToCurrencyId = x.ToCurrencyId,
                PrevRate = x.PrevRate,
                NewRate = x.NewRate,
                CreatedAt = x.CreatedAt
            })
            .OrderByDescending(x => x.CreatedAt)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetCurrencyHistoryResult(history);
    }
}