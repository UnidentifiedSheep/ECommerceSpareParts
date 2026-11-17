using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Settings;
using Attributes;
using Main.Entities.Currency.Settings;
using Main.Modules.Currency.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Modules.Currency.Application.Handlers.Currencies.UpdateCurrenciesRates;

[Transactional]
[AutoSave]
public record UpdateCurrenciesRatesCommand : ICommand;

public class UpdateCurrenciesRatesHandler(
	ILogger<UpdateCurrenciesRatesCommand> logger,
	ICurrencyRateUpdater updater,
	ISettingsService settingsService) : ICommandHandler<UpdateCurrenciesRatesCommand>
{
	public async Task<Unit> Handle(UpdateCurrenciesRatesCommand request, CancellationToken cancellationToken)
	{
		var settings = await settingsService.GetOrDefault<CurrencySetting>(cancellationToken);

		var result = await updater.UpdateAsync(settings, cancellationToken);

		if (result.NotFound.Count > 0)
			logger.LogWarning("Missing rates: {@Currencies}", result.NotFound);

		return Unit.Value;
	}
}
