using Application.Common.Interfaces.Cqrs;
using Main.Application.Dtos.Currencies;
using Main.Application.Interfaces.Cache;
using Main.Entities.Exceptions;

namespace Main.Application.Handlers.Currencies;

public record GetCurrencyByIdQuery(int Id) : IQuery<GetCurrencyByIdResult>;

public record GetCurrencyByIdResult(CurrencyDto Currency);

public class GetCurrencyByIdHandler(
    ICurrencyCacheRepository cacheRepository
)
    : IQueryHandler<GetCurrencyByIdQuery, GetCurrencyByIdResult>
{
    public async Task<GetCurrencyByIdResult> Handle(
        GetCurrencyByIdQuery request,
        CancellationToken cancellationToken)
    {
        var currency = await cacheRepository.GetCurrency(request.Id, cancellationToken)
                       ?? throw new CurrencyNotFoundException(request.Id);

        return new GetCurrencyByIdResult(currency);
    }
}