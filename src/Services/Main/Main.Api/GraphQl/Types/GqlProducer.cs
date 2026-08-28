using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Application.Dtos.Producer;

namespace Main.Api.GraphQl.Types;

[GraphQLName("Producer")]
public record GqlProducer(
    [property: GraphQLIgnore]
    ProducerDto Producer)
{
    [GraphQLName("id")]
    [Shareable]
    public int Id => Producer.Id;

    [GraphQLName("name")]
    public string Name => Producer.Name;

    [GraphQLName("description")]
    public string? Description => Producer.Description;

    [GraphQLName("aliases")]
    public async Task<IReadOnlyCollection<string>> GetAliasesAsync(
        IProducerAliasesByIdDataLoader loader,
        CancellationToken ct)
        => await loader.LoadAsync(Id, ct) ?? [];
}
