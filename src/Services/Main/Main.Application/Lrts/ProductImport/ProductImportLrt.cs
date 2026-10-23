using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Locan.Core.Interfaces;
using Main.Application.Lrts.ProductSynchronization;
using Main.Entities;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.ProductImport;

public sealed class ProductImportLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<ProductImportLrt> logger)
	: MultiStepLrtBase<ProductImportInputState, ProductImportInputState>(transactionService, logger)
{
	public override string SystemName => nameof(ProductImportLrt);

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtProductImportNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProductImportDescriptionMessage.Instance;

	protected override void ConfigureSteps(IMultiStepJobBuilder builder)
	{
		var import = builder.AddStep(
			nameof(ProductImportLrtStep),
			context => context.GetParentInput<ProductImportInputState>());

		var synchronization = builder.AddStep(
			nameof(ProductSynchronizationLrt),
			new ProductSynchronizationInputState());

		builder.AddDependency(synchronization, import);
	}
}
