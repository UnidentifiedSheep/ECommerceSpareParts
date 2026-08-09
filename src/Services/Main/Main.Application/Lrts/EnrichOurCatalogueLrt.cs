using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public class EnrichOurCatalogueLrt(
    IRepository<Job, Guid> jobRepository, 
    IUnitOfWork unitOfWork, 
    IPublishEndpoint publisher,
    ILogger logger) : MultiStepLrtBase(
    jobRepository, 
    unitOfWork,
    publisher, 
    logger)
{

    public override IServiceDefinition ServiceDefinition => ServicesDefinitions.Main;
    public override string SystemName => nameof(EnrichOurCatalogueLrt);
    public override string NameLocalizationKey { get; }
    public override string DescriptionLocalizationKey { get; }
    public override Type InputType => typeof(NoneInputState);
    public override Type StateType => typeof(NoneInputState);
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