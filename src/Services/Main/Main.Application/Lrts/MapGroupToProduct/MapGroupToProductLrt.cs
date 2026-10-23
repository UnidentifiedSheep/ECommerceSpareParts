using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Locan.Core.Interfaces;
using Main.Application.Lrts.ProductSynchronization;
using Main.Entities;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.MapGroupToProduct;

public sealed class MapGroupToProductLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<MapGroupToProductLrt> logger)
	: MultiStepLrtBase<MapGroupToProductInputState, MapGroupToProductInputState>(transactionService, logger)
{
	public override string SystemName => nameof(MapGroupToProductLrt);

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtMapGroupToProductNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtMapGroupToProductDescriptionMessage.Instance;

	protected override void ConfigureSteps(IMultiStepJobBuilder builder)
	{
		var mapping = builder.AddStep(
			nameof(MapGroupToProductLrtStep),
			context => context.GetParentInput<MapGroupToProductInputState>());
		var synchronization = builder.AddStep(nameof(ProductSynchronizationLrt), new ProductSynchronizationInputState());
		builder.AddDependency(synchronization, mapping);
	}
}
