using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Locan.Core.Interfaces;
using Main.Application.Handlers.Currencies.UpdateCurrenciesRates;
using Main.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public class UpdateCurrencyRatesLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<UpdateCurrencyRatesLrt> logger) : LrtBase<NoneInputState, NoneInputState>(
	transactionService,
	logger)
{
	public const string Name = nameof(UpdateCurrencyRatesLrt);
	public override string SystemName => Name;

	public override ILocalizableMessage NameLocalizationMessage => LrtCurrencyRatesUpdateNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtCurrencyRatesUpdateDescriptionMessage.Instance;

	protected override Task DoWork(NoneInputState inputState) =>
		ExecuteInScopeAsync((context, ct) =>
			context.GetRequiredService<ISender>().Send(new UpdateCurrenciesRatesCommand(), ct), CancellationToken);
}
