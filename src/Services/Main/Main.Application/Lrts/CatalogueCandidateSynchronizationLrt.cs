using Application.Common.Extensions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Projections;
using Application.Common.LRT;
using Contracts.Models.CatalogueCandidate;
using Contracts.ProductEnrichment;
using Locan.Core.Interfaces;
using Main.Application.Lrts.Base.Synchronization;
using Main.Entities;
using Main.Entities.Product.Enrichment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public sealed class CatalogueCandidateSynchronizationLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<CatalogueCandidateSynchronizationLrt> logger) : SynchronizationLrtBase<NoneInputState, Guid, CatalogueCandidateUpdatedEvent>(
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
		var query = TransactionContext.Repositories.GetForRead<CatalogueCandidate, Guid>().Query.AsNoTracking();
		if (lastProcessedId is { } id)
			query = query.Where(candidate => candidate.Id > id);

		var candidates = await query
			.OrderBy(candidate => candidate.Id)
			.Take(batchSize)
			.Project(TransactionContext.GetRequiredService<IProjectionProvider<CatalogueCandidate, CatalogueCandidateContractDto>>())
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
