using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Currencies;
using Main.Application.Projections;
using Main.Entities.Currency;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Currencies.GetCurrencyById;

public record GetCurrencyByIdQuery(int Id) : IQuery<GetCurrencyByIdResult>;

public record GetCurrencyByIdResult(CurrencyDto Currency);

public class GetCurrencyByIdHandler(
    IReadRepository<Currency, int> repository)
    : IQueryHandler<GetCurrencyByIdQuery, GetCurrencyByIdResult>
{
    public async Task<GetCurrencyByIdResult> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
    {
        var currency = await repository.Query
                           .AsExpandable()
                           .Select(CurrencyProjections.ToDto)
                           .FirstOrDefaultAsync(cancellationToken)
                       ?? throw new CurrencyNotFoundException(request.Id);

        return new GetCurrencyByIdResult(currency);
    }
}