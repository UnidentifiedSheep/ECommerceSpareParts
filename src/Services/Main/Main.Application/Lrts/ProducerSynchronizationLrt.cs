using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Contracts.Producer;
using Domain.CommonEntities.Job;
using Locan.Core.Interfaces;
using Main.Application.Lrts.Base.Synchronization;
using Main.Entities;
using Main.Entities.Producer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public class ProducerSynchronizationLrt(
	IRepository<Job, Guid> jobRepository,
	IReadRepository<Producer, int> producerRepository,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	ILogger<ProducerSynchronizationLrt> logger) : SynchronizationLrtBase<int, ProducerUpdatedEvent>(
	jobRepository,
	unitOfWork,
	publisher,
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
		var query = producerRepository.Query;
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
