using HotChocolate;
using Main.Application.Dtos.Producer;

namespace Main.Api.GraphQl.Types;

[GraphQLName("producer")]
public record GqlProducer(ProducerDto Producer)
{
    [GraphQLName("id")]
    public int Id => Producer.Id;

    [GraphQLName("name")]
    public string Name => Producer.Name;

    [GraphQLName("description")]
    public string? Description => Producer.Description;
}