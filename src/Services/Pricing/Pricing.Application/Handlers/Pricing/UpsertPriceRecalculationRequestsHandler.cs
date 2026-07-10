using Application.Common.Interfaces.Cqrs;
using Attributes;
using MediatR;
using Pricing.Application.Dtos.Price;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Pricing;

[Diagnostics(maxExecutionTimeMs: 400)]
[Transactional]
public record UpsertPriceRecalculationRequestsCommand(
    IEnumerable<PriceRecalculationRequestDto> RecalculationRequests
    ) : ICommand;

public class UpsertPriceRecalculationRequestsHandler(
    IPriceRecalculationRequestRepository repository
    ) : ICommandHandler<UpsertPriceRecalculationRequestsCommand>
{
    public async Task<Unit> Handle(UpsertPriceRecalculationRequestsCommand requests, CancellationToken cancellationToken)
    {
        var models = requests.RecalculationRequests
            .Select(x => PriceRecalculationRequest.Create(x.ProductId, x.StorageName))
            .ToList();
        await repository.UpsertAsync(models, cancellationToken);
        return Unit.Value;
    }
}
