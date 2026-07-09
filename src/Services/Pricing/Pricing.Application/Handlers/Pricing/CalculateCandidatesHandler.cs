using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Pricing;

[Diagnostics]
[Transactional, AutoSave]
public record CalculateCandidatesCommand(
    int ProductId,
    string StorageName) : ICommand;

public class CalculateCandidatesHandler(
    IReadRepository<PriceOffer, Guid> repository,
    IProductPriceCalculator calculator,
    IPriceCandidateBuilder builder,
    IProductPriceOptionRepository optionsRepository
    ) : ICommandHandler<CalculateCandidatesCommand>
{
    public async Task<Unit> Handle(CalculateCandidatesCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        
        var offers = await repository.Query
            .Where(x => x.ExpiresAt >= now)
            .Where(x => x.ProductId == request.ProductId)
            .Where(x => x.OfferForStorage == request.StorageName)
            .ToListAsync(cancellationToken);
        
        var candidates = await builder.Build(
            offers,
            request.StorageName,
            cancellationToken);
        
        var calculated = await calculator
            .CalculateAsync(candidates, cancellationToken);
        
        var priceOptions = calculated
            .Select(x => 
                ProductPriceOption.Create(
                    x.PriceOfferId,
                    x.StorageName,
                    x.Score,
                    x.Price,
                    x.CurrencyId,
                    x.Markup,
                    x.DeliveryTime,
                    x.GuaranteedDeliveryTime,
                    x.DeliveryProbability))
            .ToList();
        await optionsRepository.UpsertAsync(priceOptions, cancellationToken);
        return Unit.Value;
    }
}