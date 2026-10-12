using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using Locan.Core.Interfaces;
using Main.Application.Lrts.ProductSynchronization;
using Main.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.MapGroupToProduct;

public sealed class MapGroupToProductLrt(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	ILogger<MapGroupToProductLrt> logger)
	: MultiStepLrtBase<MapGroupToProductInputState, MapGroupToProductInputState>(
		jobRepository, unitOfWork, publisher, transactionService, logger)
{
	public override string SystemName => nameof(MapGroupToProductLrt);

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtMapGroupToProductNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtMapGroupToProductDescriptionMessage.Instance;

	protected override void ConfigureSteps(IMultiStepJobBuilder builder, string initialState)
	{
		var mapping = builder.AddStep(nameof(MapGroupToProductLrtStep), initialState);
		var synchronization = builder.AddStep(nameof(ProductSynchronizationLrt), NoneInputState.Json);
		builder.AddDependency(synchronization, mapping);
	}
}
