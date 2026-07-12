using Bogus;
using Enums;
using Main.Entities.Producer;
using Tests.Abstractions;

namespace Tests.DataBuilders;

public class ProducerSupplierMappingBuilder(Faker faker) : BuilderBase<ProducerSupplierMapping>(faker)
{
    private readonly HashSet<int> _producerIds = [];

    public IReadOnlyCollection<int> ProducerIds => _producerIds.AsReadOnly();

    public string? SupplierProducerName { get; private set; }
    public Supplier? Supplier { get; private set; }

    public ProducerSupplierMappingBuilder WithProducerId(int producerId)
    {
        _producerIds.Add(producerId);
        return this;
    }

    public ProducerSupplierMappingBuilder WithProducerIds(IEnumerable<int> producerIds)
    {
        _producerIds.UnionWith(producerIds);
        return this;
    }

    public ProducerSupplierMappingBuilder WithProducers(IEnumerable<Producer> producers)
    {
        _producerIds.UnionWith(producers.Select(p => p.Id));
        return this;
    }

    public ProducerSupplierMappingBuilder WithSupplierProducerName(string supplierProducerName)
    {
        SupplierProducerName = supplierProducerName;
        return this;
    }

    public ProducerSupplierMappingBuilder WithSupplier(Supplier supplier)
    {
        Supplier = supplier;
        return this;
    }

    public override ProducerSupplierMapping Build()
    {
        return ProducerSupplierMapping.Create(
            Faker.PickRandom<int>(_producerIds),
            SupplierProducerName ?? Faker.Lorem.Letter(20),
            Supplier ?? Faker.PickRandom<Supplier>());
    }
}
