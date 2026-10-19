using Application.Common.Interfaces.Persistence;
using Main.Application.Interfaces.Products;
using Locan.Core.Interfaces;
using Main.Application.Lrts.Base.Synchronization;
using Main.Entities;
using Main.Entities.Product;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.ProductSynchronization;

public sealed class ProductSynchronizationLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<ProductSynchronizationLrt> logger) : SynchronizationLrtBase<ProductSynchronizationInputState, int, object>(
	transactionService,
	logger)
{
	public const string Name = nameof(ProductSynchronizationLrt);

	public override string SystemName => Name;

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtProductSynchronizationNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProductSynchronizationDescriptionMessage.Instance;

	protected override async Task<SynchronizationPage<int, object>> GetPageAsync(
		int? lastProcessedId,
		int batchSize,
		CancellationToken cancellationToken)
	{
		var query = TransactionContext.Repositories.GetForRead<Product, int>().Query;
		if (lastProcessedId is { } id)
			query = query.Where(x => x.Id > id);

		var ids = await query
			.OrderBy(x => x.Id)
			.Select(x => x.Id)
			.Take(batchSize)
			.ToListAsync(cancellationToken);

		return new SynchronizationPage<int, object>(
			ids,
			await TransactionContext.GetRequiredService<IProductEventProvider>().GetEventsAsync(ids, InputState.Parts, cancellationToken));
	}
}
