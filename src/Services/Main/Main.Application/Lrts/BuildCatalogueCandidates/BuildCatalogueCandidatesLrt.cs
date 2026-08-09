using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using MassTransit;
using Main.Application.Handlers.ProductEnrichment.BuildCatalogueCandidatesBatch;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public class BuildCatalogueCandidatesLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    ISender sender,
    ILogger<BuildCatalogueCandidatesLrt> logger
    ) : LrtBase<NoneInputState, BuildCatalogueCandidatesState>(
    jobRepository,
    unitOfWork,
    publisher,
    logger)
{
    public const string LrtSystemName = nameof(BuildCatalogueCandidatesLrt);
    public override IServiceDefinition ServiceDefinition => ServicesDefinitions.Main;
    public override string SystemName => LrtSystemName;
    public override string NameLocalizationKey =>
        "lrt.catalogue.candidates.build.name";
    public override string DescriptionLocalizationKey =>
        "lrt.catalogue.candidates.build.description";
    protected override async Task DoWork()
    {
        const int batchSize = 1000;
        while (true)
        {
            var result = await sender.Send(
                new BuildCatalogueCandidatesBatchCommand(
                    State.LastProcessedId,
                    batchSize),
                CancellationToken);

            if (result.ReadRows == 0) return;

            await SaveStateAsync(new BuildCatalogueCandidatesState
            {
                LastProcessedId = result.LastProcessedId,
                ProcessedRows = State.ProcessedRows + result.ReadRows,
                AssignedRows = State.AssignedRows + result.AssignedRows,
                SkippedRows = State.SkippedRows + result.SkippedRows
            });

            if (!result.HasMore) break;
        }
    }
}
