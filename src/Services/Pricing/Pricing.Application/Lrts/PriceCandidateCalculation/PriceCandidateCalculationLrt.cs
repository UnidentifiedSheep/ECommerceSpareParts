using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Locan.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Pricing.Application.Handlers.Pricing;
using Pricing.Entities;

namespace Pricing.Application.Lrts.PriceCandidateCalculation;

public class PriceCandidateCalculationLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<PriceCandidateCalculationLrt> logger) : LrtBase<PriceCandidateCalculationState, PriceCandidateCalculationState>(
	transactionService,
	logger)
{
	public static string LrtName => nameof(PriceCandidateCalculationLrt);

	public override string SystemName => LrtName;

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtPriceCandidateCalculationNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtPriceCandidateCalculationDescriptionMessage.Instance;

	protected override async Task DoWork(PriceCandidateCalculationState inputState)
	{
		await ExecuteInScopeAsync((context, ct) => context.GetRequiredService<ISender>().Send(
			new CalculateCandidatesCommand(inputState.ProductId, inputState.StorageCode), ct), CancellationToken);
	}
}
