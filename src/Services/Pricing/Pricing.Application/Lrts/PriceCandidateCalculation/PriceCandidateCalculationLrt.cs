using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Locan.Core.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Pricing.Application.Handlers.Pricing;
using Pricing.Entities;

namespace Pricing.Application.Lrts.PriceCandidateCalculation;

public class PriceCandidateCalculationLrt(
	IJobRepository jobRepository,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	ILogger<PriceCandidateCalculationLrt> logger,
	ISender sender) : LrtBase<PriceCandidateCalculationState, PriceCandidateCalculationState>(
	jobRepository,
	unitOfWork,
	publisher,
	transactionService,
	logger)
{
	public static string LrtName => nameof(PriceCandidateCalculationLrt);

	public override string SystemName => LrtName;

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtPriceCandidateCalculationNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtPriceCandidateCalculationDescriptionMessage.Instance;

	protected override async Task DoWork()
	{
		await sender.Send(
			new CalculateCandidatesCommand(State.ProductId, State.StorageCode),
			CancellationToken);
	}
}
