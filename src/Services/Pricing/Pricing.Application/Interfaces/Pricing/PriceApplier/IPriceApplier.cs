using Pricing.Application.Models.Pricing;

namespace Pricing.Application.Interfaces.Pricing.PriceApplier;

public interface IPriceApplier
{
    int Order { get; }
    
    ValueTask<PriceCalculationState> ApplyAsync(
        PriceCalculationState state, 
        CancellationToken ct = default);
}