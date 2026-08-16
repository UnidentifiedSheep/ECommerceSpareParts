using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Contracts.Models.CatalogueCandidate;
using Contracts.ProductEnrichment;
using Domain.CommonEntities.Job;
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
    ILogger<CatalogueCandidateSynchronizationLrt> logger)
    : LrtBase<NoneInputState, NoneInputState>(
        jobRepository,
        unitOfWork,
        publisher,
        transactionService,
        logger)
{
    public override string SystemName => nameof(CatalogueCandidateSynchronizationLrt);

    public override string NameLocalizationKey => "lrt.catalogue.candidates.synchronization.name";

    public override string DescriptionLocalizationKey => "lrt.catalogue.candidates.synchronization.description";

    protected override async Task DoWork()
    {
        var lastId = Guid.Empty;
        const int batchSize = 1000;

        while (true)
        {
            var candidates = await candidateRepository.Query
                .Where(candidate => candidate.Id > lastId)
                .OrderBy(candidate => candidate.Id)
                .Take(batchSize)
                .Project(projection)
                .ToListAsync(CancellationToken);
            if (candidates.Count == 0) return;

            var occurredAt = DateTime.UtcNow;
            foreach (var candidate in candidates)
                await Publisher.Publish(
                    new CatalogueCandidateUpdatedEvent
                    {
                        Candidate = candidate with
                        {
                            Names = candidate.Names
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToList()
                        },
                        OccuredAt = occurredAt
                    },
                    CancellationToken);

            lastId = candidates[^1].Id;
            if (candidates.Count < batchSize) return;
        }
    }
}
