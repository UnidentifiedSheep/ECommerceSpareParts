using Abstractions.Interfaces.Services;
using Contracts.Currency.GetCurrencies;
using Main.Application.Handlers.Currencies.GetAllCurrencies;
using Mapster;
using MassTransit;
using MediatR;

namespace Main.Application.Consumers;

public class GetCurrenciesConsumer(IMediator mediator) : IConsumer<GetCurrenciesRequest>
{
    public async Task Consume(ConsumeContext<GetCurrenciesRequest> context)
    {
        var query = new GetAllCurrenciesQuery();
        var result = await mediator.Send(query);

        var currencies = result.Currencies.Adapt<List<Contracts.Models.Currency.Currency>>();
        await context.RespondAsync(new GetCurrenciesResponse { Currencies = currencies });
    }
}