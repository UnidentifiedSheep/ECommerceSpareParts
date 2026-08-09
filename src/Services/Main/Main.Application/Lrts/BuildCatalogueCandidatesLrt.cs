using System.Data;
using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Attributes;
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
    ILogger logger
    ) : LrtBase(
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
    public override Type InputType => typeof(NoneInputState);
    public override Type StateType => typeof(BuildCatalogueCandidatesState);

    protected override async Task DoWork()
    {
        const int batchSize = 1000;
        while (true)
        {
            var state = await GetStateAsync<BuildCatalogueCandidatesState>()
                        ?? new BuildCatalogueCandidatesState();
            
            var result = await sender.Send(
                new BuildCatalogueCandidatesBatchCommand(
                    state.LastProcessedId,
                    batchSize),
                CancellationToken);

            if (result.ReadRows == 0) return;

            await UpdateState(new BuildCatalogueCandidatesState
            {
                LastProcessedId = result.LastProcessedId,
                ProcessedRows = state.ProcessedRows + result.ReadRows,
                AssignedRows = state.AssignedRows + result.AssignedRows,
                SkippedRows = state.SkippedRows + result.SkippedRows
            });

            if (!result.HasMore) break;
        }
    }
}
