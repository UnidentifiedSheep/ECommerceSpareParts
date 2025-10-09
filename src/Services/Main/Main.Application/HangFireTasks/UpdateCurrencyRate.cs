using Main.Application.Handlers.Currencies.UpdateCurrenciesRates;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Application.HangFireTasks;

public class UpdateCurrencyRate(IServiceProvider serviceProvider)
{
    public async Task Run()
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new UpdateCurrenciesRatesCommand());
    }
}