using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Abstractions.Dtos.Currencies;
using Main.Abstractions.Exceptions.Currencies;
using Main.Application.Handlers.Currencies.Projections;
using Main.Entities.Currency;
using Mapster;
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
            .Select(CurrencyProjections.ToDto)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new CurrencyNotFoundException(request.Id);
        
        return new GetCurrencyByIdResult(currency);
    }
}