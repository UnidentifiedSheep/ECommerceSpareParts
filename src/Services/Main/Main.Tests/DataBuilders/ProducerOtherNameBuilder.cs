using Bogus;
using Main.Entities.Producer;
using Test.Common.Abstractions;

namespace Tests.DataBuilders;

public class ProducerOtherNameBuilder(Faker faker) : BuilderBase<ProducerOtherName>(faker)
{
    private readonly HashSet<int> _producerIds = [];
    public IReadOnlyCollection<int> ProducerIds => _producerIds.AsReadOnly();

    public string? OtherName { get; private set; }
    public string? WhereUsed { get; private set; }
    
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

    public ProducerOtherNameBuilder WithWhereUsed(string whereUsed)
    {
        WhereUsed = whereUsed;
        return this;
    }
    
    public override ProducerOtherName Build()
    {
        return ProducerOtherName.Create(
            Faker.PickRandom<int>(_producerIds),
            OtherName ?? Faker.Lorem.Letter(20),
            WhereUsed ?? Faker.Lorem.Word());
    }
}