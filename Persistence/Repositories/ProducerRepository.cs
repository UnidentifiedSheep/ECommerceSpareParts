using Core.Entities;
using Core.Interfaces.DbRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Extensions;
// ReSharper disable EntityFramework.ClientSideDbFunctionCall

namespace Persistence.Repositories;

public class ProducerRepository(DContext context) : IProducerRepository
{
    public async Task<IEnumerable<int>> ProducersExistsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var idsSet = ids.ToHashSet();
        var foundProducers = await context.Producers.AsNoTracking()
            .Where(x => idsSet.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        return idsSet.Except(foundProducers);
    }

    public async Task<Producer?> GetProducer(int producerId, bool track = true, CancellationToken cancellationToken = default)
        => await context.Producers.ConfigureTracking(track).FirstOrDefaultAsync(x => x.Id == producerId, cancellationToken);

    public async Task<bool> ProducerHasAnyArticle(int producerId, CancellationToken cancellationToken = default)
    {
        return await context.Articles.AsNoTracking().AnyAsync(x => x.ProducerId == producerId, cancellationToken);
    }

    public async Task<bool> OtherNameIsTaken(string otherName, int? producerId = null, string? whereUsed = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.ProducersOtherNames
            .AsNoTracking()
            .Where(x => x.ProducerOtherName == otherName)
            .Where(x => x.WhereUsed == whereUsed);

        if (producerId != null)
            query = query.Where(x => x.ProducerId == producerId);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsProducerNameTaken(string producerName, CancellationToken cancellationToken = default)
        => await context.Producers.AsNoTracking().AnyAsync(x => x.Name == producerName, cancellationToken);

    public async Task<ProducersOtherName?> GetOtherName(int producerId, string otherName, string? whereUsed,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ProducersOtherNames.ConfigureTracking(track)
            .Where(x => x.ProducerId == producerId)
            .Where(x => x.WhereUsed == whereUsed)
            .Where(x => x.ProducerOtherName == otherName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Producer>> GetProducers(string? searchTerm, int page, int viewCount, bool track = true,
        CancellationToken cancellationToken = default)
    {
        var query = context.Producers.ConfigureTracking(track);
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{searchTerm}%"));

        var queryWithRang = query.Select(z => new
            {
                Producer = z,
                Rank = string.IsNullOrWhiteSpace(searchTerm) ? 0 : EF.Functions.TrigramsSimilarity(z.Name, searchTerm)
            })
            .OrderByDescending(x => x.Rank);
        
        return await queryWithRang.Skip(page * viewCount)
            .Select(x => x.Producer)
            .Take(viewCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProducersOtherName>> GetOtherNames(int producerId, int page, int viewCount, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ProducersOtherNames.ConfigureTracking(track)
            .Where(x => x.ProducerId == producerId)
            .Skip(viewCount * page)
            .Take(viewCount)
            .ToListAsync(cancellationToken);
    }
}