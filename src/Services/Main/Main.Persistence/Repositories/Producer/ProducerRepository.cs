using Main.Application.Interfaces.Persistence;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence;

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
}