using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Settings;
using Attributes;
using Contracts.Currency;
using Main.Application.Interfaces.Services.Currency;
using Main.Entities.Settings;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Application.Handlers.Currencies.UpdateCurrenciesRates;

[Transactional]
[AutoSave]
public record UpdateCurrenciesRatesCommand : ICommand;

public class UpdateCurrenciesRatesHandler(
    ILogger<UpdateCurrenciesRatesCommand> logger,
    ICurrencyRateUpdater updater,
    IIntegrationEventScope integrationEventScope,
    ISettingsService settingsService
) : ICommandHandler<UpdateCurrenciesRatesCommand>
{
    public async Task<Unit> Handle(UpdateCurrenciesRatesCommand request, CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetOrDefault<CurrencySetting>(cancellationToken);

        var result = await updater.UpdateAsync(settings, cancellationToken);

        if (result.NotFound.Count > 0) logger.LogWarning("Missing rates: {@Currencies}", result.NotFound);

        if (result.Changed.Count > 0)
            integrationEventScope.Add(new CurrencyRateChangedEvent { Rates = result.Changed });

        return Unit.Value;
    }
}