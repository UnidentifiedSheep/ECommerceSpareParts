using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Persistence;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Extensions;

namespace Main.Persistence.Repositories.Producer;

public class ProducerRepository(DContext context) 
    : RepositoryBase<DContext, Entities.Producer.Producer, int>(context), IProducerRepository
{
    public async Task<bool> ProducerHasAnyArticle(int producerId, CancellationToken cancellationToken = default)
    {
        return await Context.Products
            .AsNoTracking()
            .AnyAsync(x => x.ProducerId == producerId, cancellationToken);
    }

    public override Task<Dictionary<int, Entities.Producer.Producer>> FindByIdsAsync(
        IEnumerable<int> ids, 
        Criteria<Entities.Producer.Producer>? criteria = null, 
        CancellationToken ct = default)
    {
        return Context.Producers
            .Apply(criteria)
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
    }
}