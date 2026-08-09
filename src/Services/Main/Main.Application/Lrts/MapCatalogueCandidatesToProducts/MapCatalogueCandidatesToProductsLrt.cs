using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using MassTransit;
using Main.Application.Handlers.ProductEnrichment.MapCatalogueCandidatesToProductsBatch;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public class MapCatalogueCandidatesToProductsLrt(
    IRepository<Job, Guid> jobRepository, 
    IUnitOfWork unitOfWork, 
    IPublishEndpoint publisher,
    ISender sender,
    ILogger<MapCatalogueCandidatesToProductsLrt> logger)
    : LrtBase<NoneInputState, MapCatalogueCandidatesToProductsState>(
    jobRepository, 
    unitOfWork,
    publisher, 
    logger)
{
    public const string LrtSystemName = nameof(MapCatalogueCandidatesToProductsLrt);
    public override IServiceDefinition ServiceDefinition => ServicesDefinitions.Main;
    public override string SystemName => LrtSystemName;
    public override string NameLocalizationKey =>
        "lrt.catalogue.candidates.map.to.products.name";
    public override string DescriptionLocalizationKey =>
        "lrt.catalogue.candidates.map.to.products.description";

    protected override async Task DoWork()
    {
        const int batchSize = 1000;

        while (true)
        {
            var result = await sender.Send(
                new MapCatalogueCandidatesToProductsBatchCommand(
                    State.LastProcessedId,
                    batchSize),
                CancellationToken);

            if (result.ReadRows == 0) return;

            await SaveStateAsync(new MapCatalogueCandidatesToProductsState
            {
                LastProcessedId = result.LastProcessedId,
                ProcessedRows = State.ProcessedRows + result.ReadRows,
                MappedRows = State.MappedRows + result.MappedRows,
                SkippedRows = State.SkippedRows + result.SkippedRows
            });

            if (!result.HasMore) break;
        }
    }
}
