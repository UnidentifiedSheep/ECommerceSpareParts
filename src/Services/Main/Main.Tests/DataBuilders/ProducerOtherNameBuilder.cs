using Bogus;
using Main.Entities.Producer;
using Test.Common.Abstractions;

namespace Tests.DataBuilders;

public class ProducerOtherNameBuilder(Faker faker) : BuilderBase<ProducerAlias>(faker)
{
    private readonly HashSet<int> _producerIds = [];
    public IReadOnlyCollection<int> ProducerIds => _producerIds.AsReadOnly();

    public string? OtherName { get; private set; }

    public ProducerOtherNameBuilder WithProducerId(int producerId)
    {
        _producerIds.Add(producerId);
        return this;
    }

    public ProducerOtherNameBuilder WithProducerIds(IEnumerable<int> producerIds)
    {
        _producerIds.UnionWith(producerIds);
        return this;
    }

    public ProducerOtherNameBuilder WithProducers(IEnumerable<Producer> producers)
    {
        _producerIds.UnionWith(producers.Select(p => p.Id));
        return this;
    }

    public ProducerOtherNameBuilder WithOtherName(string otherName)
    {
        OtherName = otherName;
        return this;
    }

    public override ProducerAlias Build()
    {
        return ProducerAlias.Create(
            Faker.PickRandom<int>(_producerIds),
            OtherName ?? Faker.Lorem.Letter(20));
    }
}