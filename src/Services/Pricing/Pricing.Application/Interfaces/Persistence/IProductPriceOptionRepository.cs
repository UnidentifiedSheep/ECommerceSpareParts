using Application.Common.Interfaces.Repositories;
using Pricing.Entities;
using Pricing.Entities.Offers;

namespace Pricing.Application.Interfaces.Persistence;

public interface IProductPriceOptionRepository : IRepository<ProductPriceOption, Guid>
{
    Task UpsertAsync(
        IEnumerable<ProductPriceOption> options,
        CancellationToken cancellationToken = default);
}