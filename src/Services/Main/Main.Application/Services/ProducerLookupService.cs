using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Producer;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Services;

public class ProducerLookupService(
    IReadRepository<Producer, int> producerReadRepository) : IProducerLookupService
{
    public async Task<ProducerLookup> Load(CancellationToken cancellationToken = default)
    {
        var producerNamesToIds = new Dictionary<string, int>();
        var aliasesToIds = new Dictionary<string, int>();

        const int batchSize = 1000;

        var baseQuery = producerReadRepository.Query
            .Select(x => new
            {
                id = x.Id,
                name = x.Name,
                aliases = x.Aliases.Select(z => z.Alias)
            })
            .OrderBy(x => x.id);

        var lastId = 0;

        while (true)
        {
            var id = lastId;
            var producers = await baseQuery
                .Where(x => x.id > id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (producers.Count == 0) break;

            lastId = producers.Last().id;

            foreach (var item in producers)
            {
                producerNamesToIds.TryAdd(item.name, item.id);
                foreach (var alias in item.aliases) aliasesToIds.TryAdd(alias, item.id);
            }

            if (producers.Count != batchSize) break;
        }

        return new ProducerLookup(producerNamesToIds, aliasesToIds);
    }
}
