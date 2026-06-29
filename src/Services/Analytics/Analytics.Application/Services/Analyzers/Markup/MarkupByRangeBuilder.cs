using Abstractions.Interfaces.Persistence;
using Analytics.Entities;
using Application.Common.Interfaces.Repositories;

namespace Analytics.Application.Services.Analyzers.Markup;

public class MarkupByRangeBuilder(
    IRepository<SalesFact, Guid> repository,
    IUnitOfWork unitOfWork)
{
    private const decimal MaxCostRation = 1.5m;
    
    public async Task CalculateAsync(CancellationToken cancellationToken = default)
    {
        var criteria = Criteria<SalesFact>
            .New()
            .Include(x => x.SaleContents);
        var currentBucket = new MarkupBucketBuilder();

        await foreach (var sale in repository.AsyncEnumerable()
                           .WithCancellation(cancellationToken)) ;
    }
}