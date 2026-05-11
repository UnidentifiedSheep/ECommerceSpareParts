using Bogus;
using Main.Entities.Producer;
using Test.Common.Abstractions;

namespace Tests.DataBuilders;

public class ProducerBuilder(Faker faker) : BuilderBase<Producer>(faker)
{
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }

    public ProducerBuilder WithName(string name)
    {
        Name = name;
        return this;
    }

    public ProducerBuilder WithDescription(string? description)
    {
        Description = description;
        return this;
    }

    public ProducerBuilder WithImageUrl(string? imageUrl)
    {
        ImageUrl = imageUrl;
        return this;
    }

    public override Producer Build()
    {
        return Producer.Create(
            Name ?? Faker.Lorem.Letter(40),
            Description,
            ImageUrl);
    }
}