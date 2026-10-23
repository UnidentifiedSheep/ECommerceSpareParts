using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Locan.Core.Interfaces;
using Main.Application.Interfaces.Products;
using Main.Application.Lrts.ProductSynchronization;
using Main.Entities;
using Main.Enums;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.ProductCharacteristicsImport;

public sealed class ProductCharacteristicsImportLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<ProductCharacteristicsImportLrt> logger)
	: MultiStepLrtBase<ProductCharacteristicsImportInputState, ProductCharacteristicsImportInputState>(transactionService, logger)
{
	public const string Name = nameof(ProductCharacteristicsImportLrt);

	public override string SystemName => Name;
	public override ILocalizableMessage NameLocalizationMessage => LrtProductCharacteristicsImportNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage => LrtProductCharacteristicsImportDescriptionMessage.Instance;

	protected override void ConfigureSteps(IMultiStepJobBuilder builder)
	{
		var import = builder.AddStep(
			ProductCharacteristicsImportLrtStep.Name,
			context => context.GetParentInput<ProductCharacteristicsImportInputState>());

		var synchronization = builder.AddStep(
			ProductSynchronizationLrt.Name,
			new ProductSynchronizationInputState { Parts = ProductEventParts.Characteristics });

		builder.AddDependency(synchronization, import);
	}
}
