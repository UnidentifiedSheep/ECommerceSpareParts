using System.Collections.Frozen;
using Enums;
using Main.Application.Interfaces.Services;
using ProducerEntity = Main.Entities.Producer.Producer;

namespace Main.Application.Models.Producer;

public class ProducerLookup(
	IReadOnlyDictionary<string, int> producerNamesToIds,
	IReadOnlyDictionary<string, int> aliasesToIds) : IProducerLookup
{
	private readonly FrozenDictionary<string, int> _producerNamesToIds = producerNamesToIds.ToFrozenDictionary(StringComparer.Ordinal);
	private readonly FrozenDictionary<string, int> _aliasesToIds = aliasesToIds.ToFrozenDictionary(StringComparer.Ordinal);

	public static ProducerLookup Empty { get; } = new(
		new Dictionary<string, int>(),
		new Dictionary<string, int>());

	public int? ResolveId(string producer, Supplier? supplier = null)
	{
		if (string.IsNullOrWhiteSpace(producer))
			return null;

		var normalizedProducer = ProducerEntity.ToNormalizedName(producer);

		if (_producerNamesToIds.TryGetValue(normalizedProducer, out var producerId))
			return producerId;

		return _aliasesToIds.TryGetValue(normalizedProducer, out var aliasProducerId) ? aliasProducerId : null;
	}
}
