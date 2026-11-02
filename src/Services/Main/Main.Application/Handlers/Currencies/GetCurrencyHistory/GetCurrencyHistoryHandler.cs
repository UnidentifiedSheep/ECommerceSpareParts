using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Application.Dtos.Currencies;
using Main.Entities.Currency;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Currencies.GetCurrencyHistory;

public record GetCurrencyHistoryQuery(int CurrencyId, Pagination Pagination)
	: IQuery<GetCurrencyHistoryResult>;

public record GetCurrencyHistoryResult(IReadOnlyList<CurrencyRateHistoryDto> History);

public class GetCurrencyHistoryHandler(IReadRepository<CurrencyRate, int> repository)
	: IQueryHandler<GetCurrencyHistoryQuery, GetCurrencyHistoryResult>
{
	public async Task<GetCurrencyHistoryResult> Handle(
		GetCurrencyHistoryQuery request,
		CancellationToken cancellationToken)
	{
		var history = await repository
			.Query
			.Where(x => x.FromCurrencyId == request.CurrencyId || x.ToCurrencyId == request.CurrencyId)
			.Select(x => new CurrencyRateHistoryDto
			{
				Id = x.Id,
				FromCurrencyId = x.FromCurrencyId,
				ToCurrencyId = x.ToCurrencyId,
				Rate = x.Rate,
				CreatedAt = x.CreatedAt
			})
			.OrderByDescending(x => x.CreatedAt)
			.ThenByDescending(x => x.Id)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetCurrencyHistoryResult(history);
	}
}
