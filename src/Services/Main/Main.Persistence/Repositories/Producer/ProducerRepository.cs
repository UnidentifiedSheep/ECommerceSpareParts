using EFCore.BulkExtensions;
using Main.Application.Interfaces.Persistence;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Repository;
using QueryExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Main.Persistence.Repositories.Producer;

public class ProducerRepository(DContext context, QueryExtensions extensions)
    : LinqRepositoryBase<DContext, Entities.Producer.Producer, int>(context, extensions), IProducerRepository
{
    public Task<bool> ProducerHasAnyArticle(int producerId, CancellationToken cancellationToken = default)
    {
        return Context.Products
            .AsNoTracking()
            .AnyAsync(x => x.ProducerId == producerId, cancellationToken);
    }

    public async Task BulkInsertOnConflictDoNothing(
        IEnumerable<Entities.Producer.Producer> producers,
        CancellationToken cancellationToken = default)
    {
        var producerList = producers.ToList();

        if (producerList.Count == 0) return;

        await Context.BulkInsertAsync(
            producerList,
            new BulkConfig
            {
                ConflictOption = ConflictOption.Ignore
            },
            cancellationToken: cancellationToken);
    }
}