using ProducerEntity = Main.Entities.Producer.Producer;

namespace Main.Application.Models.Producer;

public class ProducerLookup(
    IReadOnlyDictionary<string, int> producerNamesToIds,
    IReadOnlyDictionary<string, int> aliasesToIds)
{
    public static ProducerLookup Empty { get; } = new(
        new Dictionary<string, int>(),
        new Dictionary<string, int>());

    public int? ResolveId(string producer)
    {
        if (string.IsNullOrWhiteSpace(producer)) return null;

        var normalizedProducer = ProducerEntity.ToNormalizedName(producer);

        if (producerNamesToIds.TryGetValue(normalizedProducer, out var producerId)) return producerId;

        return aliasesToIds.TryGetValue(normalizedProducer, out var aliasProducerId)
            ? aliasProducerId
            : null;
    }
}
