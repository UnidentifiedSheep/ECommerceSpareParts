using Bogus;
using Main.Entities.Producer;
using Test.Common.Abstractions;

namespace Tests.DataBuilders;

public class ProducerAliasBuilder(Faker faker) : BuilderBase<ProducerAlias>(faker)
{
    private readonly HashSet<int> _producerIds = [];
    public IReadOnlyCollection<int> ProducerIds => _producerIds.AsReadOnly();

    public string? Alias { get; private set; }

    public ProducerAliasBuilder WithProducerId(int producerId)
    {
        _producerIds.Add(producerId);
        return this;
    }

    public ProducerAliasBuilder WithProducerIds(IEnumerable<int> producerIds)
    {
        _producerIds.UnionWith(producerIds);
        return this;
    }

    public ProducerAliasBuilder WithProducers(IEnumerable<Producer> producers)
    {
        _producerIds.UnionWith(producers.Select(p => p.Id));
        return this;
    }

    public ProducerAliasBuilder WithAlias(string otherName)
    {
        Alias = otherName;
        return this;
    }

    public override ProducerAlias Build()
    {
        return ProducerAlias.Create(
            Faker.PickRandom<int>(_producerIds),
            Alias ?? Faker.Lorem.Letter(20));
    }
}