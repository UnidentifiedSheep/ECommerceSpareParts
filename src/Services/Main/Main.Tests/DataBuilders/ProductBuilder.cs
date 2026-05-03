using System.Collections;
using Bogus;
using Main.Entities.Producer;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;
using Test.Common.Abstractions;


namespace Tests.DataBuilders;

public class ProductBuilder(Faker faker) : BuilderBase<Product>(faker)
{
    public Sku? Sku { get; private set; }
    public Name? Name { get; private set; }
    public string? Description { get; private set; }
    private readonly HashSet<int> _producerIds = [];
    public IReadOnlyCollection<int> ProducerIds => _producerIds.AsReadOnly();

    public ProductBuilder WithSku(Sku sku)
    {
        Sku = sku;
        return this;
    }

    public ProductBuilder WithName(Name name)
    {
        Name = name;
        return this;
    }

    public ProductBuilder WithProducerId(int producerId)
    {
        _producerIds.Add(producerId);
        return this;
    }

    public ProductBuilder WithProducerIds(IEnumerable<int> producerIds)
    {
        _producerIds.UnionWith(producerIds);
        return this;
    }
    
    public ProductBuilder WithProducers(IEnumerable<Producer> producers)
    {
        _producerIds.UnionWith(producers.Select(p => p.Id));
        return this;
    }

    public ProductBuilder WithDescription(string? description)
    {
        Description = description;
        return this;
    }
    
    public override Product Build()
    {
        return Product.Create(
            Sku ?? Faker.Lorem.Letter(10), 
            Name ?? Faker.Commerce.ProductName(), 
            Faker.PickRandom<int>(ProducerIds), 
            Description);
    }
}