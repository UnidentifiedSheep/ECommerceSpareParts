using Application.Common.Interfaces.Repositories;
using Pricing.Entities;

namespace Pricing.Application.Services.Pricing;

public class BasePriceCalculator()
{
    public async Task GetBasePriceAsync(
        int productId,
        int currencyId,
        CancellationToken token)
    {
        
    }
}