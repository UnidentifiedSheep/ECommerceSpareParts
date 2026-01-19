using Application.Common.Interfaces;
using Exceptions.Exceptions.Currencies;
using Main.Abstractions.Dtos.Currencies;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Currencies.GetCurrencyById;

public record GetCurrencyByIdQuery(int Id) : IQuery<GetCurrencyByIdResult>;
public record GetCurrencyByIdResult(CurrencyDto Currency);

public class GetCurrencyByIdHandler(ICurrencyRepository currencyRepository) : IQueryHandler<GetCurrencyByIdQuery, GetCurrencyByIdResult>
{
    public async Task<GetCurrencyByIdResult> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
    {
        var currency = await currencyRepository.GetCurrencyById(request.Id, false, cancellationToken);
        return currency == null ? throw new CurrencyNotFoundException(request.Id) : new GetCurrencyByIdResult(currency.Adapt<CurrencyDto>());
    }
}