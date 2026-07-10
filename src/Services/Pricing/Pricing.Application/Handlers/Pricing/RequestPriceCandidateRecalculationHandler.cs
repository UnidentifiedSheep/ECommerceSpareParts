using System.Text.Json;
using Application.Common.Handlers.Jobs;
using Application.Common.Interfaces.Cqrs;
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
    ISender sender
    ) : ICommandHandler<RequestPriceCandidateRecalculationCommand>
{
    public async Task<Unit> Handle(RequestPriceCandidateRecalculationCommand requests, CancellationToken cancellationToken)
    {
        var items = requests.RecalculationRequests
            .Select(x => PriceCandidateCalculationJob
                .Create(x.ProductId, x.StorageName))
            .ToList();
        
        await sender.Send(new TryEnqueueUniqJobCommand(items), cancellationToken);
        return Unit.Value;
    }
}
