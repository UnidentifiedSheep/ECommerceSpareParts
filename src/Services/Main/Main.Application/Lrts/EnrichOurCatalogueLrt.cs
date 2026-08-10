using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using Main.Application.Lrts.BuildCatalogueCandidates;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public class EnrichOurCatalogueLrt(
    IRepository<Job, Guid> jobRepository, 
    IUnitOfWork unitOfWork, 
    IPublishEndpoint publisher,
    IApplicationTransactionService transactionService,
    ILogger<EnrichOurCatalogueLrt> logger) : MultiStepLrtBase<NoneInputState, NoneInputState>(
    jobRepository, 
    unitOfWork,
    publisher, 
    transactionService,
    logger)
{

    public override string SystemName => nameof(EnrichOurCatalogueLrt);
    public override string NameLocalizationKey => "lrt.catalogue.enrichment.name";
    public override string DescriptionLocalizationKey => "lrt.catalogue.enrichment.description";
    protected override void ConfigureSteps(
        IMultiStepJobBuilder builder, 
        string initialState)
    {
        var buildCandidatesStep = builder.AddStep(
            BuildCatalogueCandidatesLrt.LrtSystemName,
            NoneInputState.Json);
        
        var mapToCatalogueStep = builder.AddStep(
            MapCatalogueCandidatesToProductsLrt.LrtSystemName,
            NoneInputState.Json);
        
        builder.AddDependency(mapToCatalogueStep, buildCandidatesStep);
    }
}
