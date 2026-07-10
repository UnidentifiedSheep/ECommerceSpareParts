using Application.Common.Interfaces.Repositories;
using Pricing.Entities;

namespace Pricing.Application.Interfaces.Persistence;

public interface IPriceRecalculationRequestRepository : IRepository<PriceRecalculationRequest, PriceRecalculationRequestKey>
{
    Task UpsertAsync(
        IEnumerable<PriceRecalculationRequest> requests,
        CancellationToken cancellationToken = default);
}