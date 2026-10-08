using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using Locan.Core.Interfaces;
using Main.Application.Handlers.Currencies.UpdateCurrenciesRates;
using Main.Entities;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public class UpdateCurrencyRatesLrt(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	ILogger<UpdateCurrencyRatesLrt> logger,
	ISender sender) : LrtBase<NoneInputState, NoneInputState>(
	jobRepository,
	unitOfWork,
	publisher,
	transactionService,
	logger)
{
	public const string Name = nameof(UpdateCurrencyRatesLrt);
	public override string SystemName => Name;

	public override ILocalizableMessage NameLocalizationMessage => LrtCurrencyRatesUpdateNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtCurrencyRatesUpdateDescriptionMessage.Instance;

	protected override Task DoWork(NoneInputState inputState)
		=> sender.Send(new UpdateCurrenciesRatesCommand(), CancellationToken);
}
