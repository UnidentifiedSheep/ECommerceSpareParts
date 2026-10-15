using Application.Common.Extensions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Contracts.Models.CatalogueCandidate;
using Contracts.ProductEnrichment;
using Domain.CommonEntities.Job;
using Locan.Core.Interfaces;
using Main.Application.Lrts.Base.Synchronization;
using Main.Entities;
using Main.Entities.Product.Enrichment;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public sealed class CatalogueCandidateSynchronizationLrt(
	IRepository<Job, Guid> jobRepository,
	IReadRepository<CatalogueCandidate, Guid> candidateRepository,
	IProjectionProvider<CatalogueCandidate, CatalogueCandidateContractDto> projection,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	ILogger<CatalogueCandidateSynchronizationLrt> logger) : SynchronizationLrtBase<NoneInputState, Guid, CatalogueCandidateUpdatedEvent>(
	jobRepository,
	unitOfWork,
	publisher,
	transactionService,
	logger)
{
	public override string SystemName => nameof(CatalogueCandidateSynchronizationLrt);

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtCatalogueCandidatesSynchronizationNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtCatalogueCandidatesSynchronizationDescriptionMessage.Instance;

	protected override async Task<SynchronizationPage<Guid, CatalogueCandidateUpdatedEvent>> GetPageAsync(
		Guid? lastProcessedId,
		int batchSize,
		CancellationToken cancellationToken)
	{
		var query = candidateRepository.Query.AsNoTracking();
		if (lastProcessedId is { } id)
			query = query.Where(candidate => candidate.Id > id);

		var candidates = await query
			.OrderBy(candidate => candidate.Id)
			.Take(batchSize)
			.Project(projection)
			.ToListAsync(cancellationToken);

		var occurredAt = DateTime.UtcNow;
		return new SynchronizationPage<Guid, CatalogueCandidateUpdatedEvent>(
			candidates.Select(candidate => candidate.Id).ToList(),
			candidates.Select(candidate => new CatalogueCandidateUpdatedEvent
			{
				Candidate = candidate with
				{
					Names = candidate.Names.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
				},
				OccuredAt = occurredAt
			}).ToList());
	}
}
