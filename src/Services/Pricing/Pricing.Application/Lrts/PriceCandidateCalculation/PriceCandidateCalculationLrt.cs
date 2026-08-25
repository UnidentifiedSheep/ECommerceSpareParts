using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Application.Common.NamedObject;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Pricing.Application.Handlers.Pricing;

namespace Pricing.Application.Lrts.PriceCandidateCalculation;

public class PriceCandidateCalculationLrt(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    IApplicationTransactionService transactionService,
    ILogger<PriceCandidateCalculationLrt> logger,
    ISender sender
) : LrtBase<PriceCandidateCalculationState, PriceCandidateCalculationState>(
    jobRepository,
    unitOfWork,
    publisher,
    transactionService,
    logger)
{
    public static string LrtName => nameof(PriceCandidateCalculationLrt);
    public override string SystemName => LrtName;
    public override string NameLocalizationKey => "lrt.price.candidate.calculation.name";
    public override string DescriptionLocalizationKey => "lrt.price.candidate.calculation.description";
    protected override async Task DoWork()
    {
        await sender.Send(new CalculateCandidatesCommand(
                State.ProductId,
                State.StorageCode),
            CancellationToken);
    }
}
