using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Contracts.Products;
using Domain.CommonEntities.Job;
using Locan.Core.Interfaces;
using Main.Application.Lrts.Base.Synchronization;
using Main.Entities;
using Main.Entities.Product;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.ProductSynchronization;

public sealed class ProductSynchronizationLrt(
	IRepository<Job, Guid> jobRepository,
	IReadRepository<Product, int> productRepository,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	ILogger<ProductSynchronizationLrt> logger) : SynchronizationLrtBase<int, ProductUpdatedEvent>(
	jobRepository,
	unitOfWork,
	publisher,
	transactionService,
	logger)
{

	public override string SystemName => nameof(ProductSynchronizationLrt);

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtProductSynchronizationNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProductSynchronizationDescriptionMessage.Instance;

	protected override async Task<IReadOnlyList<SynchronizationEvent<int, ProductUpdatedEvent>>> GetEventsAsync(
		int? lastProcessedId,
		int batchSize,
		CancellationToken cancellationToken)
	{
		var query = productRepository.Query.AsNoTracking();
		if (lastProcessedId is { } id)
			query = query.Where(x => x.Id > id);

		var ids = await query
			.OrderBy(x => x.Id)
			.Select(x => x.Id)
			.Take(batchSize)
			.ToListAsync(cancellationToken);

		return ids.Select(id => new SynchronizationEvent<int, ProductUpdatedEvent>(
			id, new ProductUpdatedEvent { Id = id })).ToList();
	}
}
