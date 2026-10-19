using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Locan.Core.Interfaces;
using Main.Application.Lrts.BuildCatalogueCandidates;
using Main.Application.Lrts.MapCatalogueCandidatesToProducts;
using Main.Entities;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public class EnrichOurCatalogueLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<EnrichOurCatalogueLrt> logger) : MultiStepLrtBase<NoneInputState, NoneInputState>(
	transactionService,
	logger)
{

	public override string SystemName => nameof(EnrichOurCatalogueLrt);

	public override ILocalizableMessage NameLocalizationMessage => LrtCatalogueEnrichmentNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtCatalogueEnrichmentDescriptionMessage.Instance;

	protected override void ConfigureSteps(IMultiStepJobBuilder builder, string initialState)
	{
		var buildCandidatesStep = builder.AddStep(
			BuildCatalogueCandidatesLrt.LrtSystemName,
			NoneInputState.Json);

		var mapToCatalogueStep = builder.AddStep(
			MapCatalogueCandidatesToProductsLrt.LrtSystemName,
			NoneInputState.Json);

		builder.AddDependency(mapToCatalogueStep, buildCandidatesStep);
	}
}
