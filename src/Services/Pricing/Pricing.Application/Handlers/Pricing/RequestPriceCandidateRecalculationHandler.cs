using System.Text.Json;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Services;
using Attributes;
using MediatR;
using Pricing.Application.Dtos.Price;
using Pricing.Application.Models.Jobs;

namespace Pricing.Application.Handlers.Pricing;

[Diagnostics(maxExecutionTimeMs: 400)]
[Transactional]
public record RequestPriceCandidateRecalculationCommand(
    IEnumerable<PriceRecalculationRequestDto> RecalculationRequests
    ) : ICommand;

public class RequestPriceCandidateRecalculationHandler(
    IJobService jobService
    ) : ICommandHandler<RequestPriceCandidateRecalculationCommand>
{
    public async Task<Unit> Handle(RequestPriceCandidateRecalculationCommand requests, CancellationToken cancellationToken)
    {
        var items = requests.RecalculationRequests
            .Select(x => PriceCandidateCalculationJob
                .Create(x.ProductId, x.StorageName))
            .ToList();
        
        await jobService.TryEnqueueJobsAsync(items, cancellationToken);
        return Unit.Value;
    }
}
