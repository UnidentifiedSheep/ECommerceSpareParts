using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Services;
using Attributes;
using MediatR;
using Pricing.Application.Dtos.Price;
using Pricing.Application.Lrts.PriceCandidateCalculation;
using Pricing.Application.Models.Jobs;

namespace Pricing.Application.Handlers.Pricing;

[Transactional]
public record RequestPriceCandidateRecalculationCommand(
    IEnumerable<PriceRecalculationRequestDto> RecalculationRequests
    ) : ICommand;

public class RequestPriceCandidateRecalculationHandler(
    IJobService jobService,
    IJobProvider<PriceCandidateCalculationLrt, PriceCandidateCalculationState> jobProvider
    ) : ICommandHandler<RequestPriceCandidateRecalculationCommand>
{
    public async Task<Unit> Handle(RequestPriceCandidateRecalculationCommand requests, CancellationToken cancellationToken)
    {
        var items = requests.RecalculationRequests
            .Select(x => jobProvider.Create(new PriceCandidateCalculationState
            {
                ProductId = x.ProductId,
                StorageCode = x.StorageCode
            }))
            .ToList();
        
        await jobService.TryEnqueueJobsAsync(items, cancellationToken);
        return Unit.Value;
    }
}
