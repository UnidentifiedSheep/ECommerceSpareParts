using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Contracts.Producer;
using Locan.Core.Interfaces;
using Main.Application.Lrts.Base.Synchronization;
using Main.Entities;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public class ProducerSynchronizationLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<ProducerSynchronizationLrt> logger) : SynchronizationLrtBase<
	NoneInputState,
	int,
	ProducerUpdatedEvent>(
	transactionService,
	logger)
{

	public override string SystemName => nameof(ProducerSynchronizationLrt);

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtProducerSynchronizationNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProducerSynchronizationDescriptionMessage.Instance;

	protected override async Task<SynchronizationPage<int, ProducerUpdatedEvent>> GetPageAsync(
		int? lastProcessedId,
		int batchSize,
		CancellationToken cancellationToken)
	{
		var query = TransactionContext.Repositories.GetForRead<Producer, int>().Query;
		if (lastProcessedId is { } id)
			query = query.Where(x => x.Id > id);

		var ids = await query
			.OrderBy(x => x.Id)
			.Select(x => x.Id)
			.Take(batchSize)
			.ToListAsync(cancellationToken);

		return new SynchronizationPage<int, ProducerUpdatedEvent>(
			ids,
			ids.Select(x => new ProducerUpdatedEvent { Id = x }).ToList());
	}
}
