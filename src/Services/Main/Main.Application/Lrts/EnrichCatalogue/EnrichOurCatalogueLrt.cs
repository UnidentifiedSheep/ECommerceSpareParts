using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Locan.Core.Interfaces;
using Main.Application.Lrts.EnrichCatalogue.BuildCatalogueCandidates;
using Main.Application.Lrts.EnrichCatalogue.MapCatalogueCandidatesToProducts;
using Main.Entities;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.EnrichCatalogue;

public class EnrichOurCatalogueLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<EnrichOurCatalogueLrt> logger) : MultiStepLrtBase<NoneInputState, NoneInputState>(
	transactionService,
	logger)
{
	public const string Name = nameof(EnrichOurCatalogueLrt);
	public override string SystemName => Name;

	public override ILocalizableMessage NameLocalizationMessage => LrtCatalogueEnrichmentNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtCatalogueEnrichmentDescriptionMessage.Instance;

	protected override void ConfigureSteps(IMultiStepJobBuilder builder)
	{
		var buildCandidatesStep = builder.AddStep(
			BuildCatalogueCandidatesLrt.LrtSystemName,
			new NoneInputState());

		var mapToCatalogueStep = builder.AddStep(
			MapCatalogueCandidatesToProductsLrt.LrtSystemName,
			new NoneInputState());

		builder.AddDependency(mapToCatalogueStep, buildCandidatesStep);
	}
}
