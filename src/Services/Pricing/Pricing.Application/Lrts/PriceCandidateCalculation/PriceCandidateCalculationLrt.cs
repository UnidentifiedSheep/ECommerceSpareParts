using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Pricing.Application.Handlers.Pricing;

namespace Pricing.Application.Lrts.PriceCandidateCalculationLrt;

public class PriceCandidateCalculationLrt(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    ILogger<PriceCandidateCalculationLrt> logger,
    ISender sender
) : LrtNamedObjectBase(
    jobRepository,
    unitOfWork,
    publisher,
    logger)
{
    public static string LrtName => nameof(PriceCandidateCalculationLrt);
    public override string SystemName => LrtName;
    public override IServiceDefinition ServiceDefinition => ServicesDefinitions.Pricing;
    public override Type InputType => typeof(PriceCandidateCalculationState);
    public override Type StateType => typeof(PriceCandidateCalculationState);
    public override string NameLocalizationKey => "lrt.price.candidate.calculation.name";
    public override string DescriptionLocalizationKey => "lrt.price.candidate.calculation.description";
    protected override async Task DoWork()
    {
        var state = await GetStateAsync<PriceCandidateCalculationState>()
                    ?? throw new InvalidOperationException($"{GetType().Name} state is empty.");

        await sender.Send(new CalculateCandidatesCommand(
                state.ProductId,
                state.StorageName),
            CancellationToken);
    }
}
