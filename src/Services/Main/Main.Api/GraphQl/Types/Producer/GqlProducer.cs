using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Application.Dtos.Producer;
using Main.Entities.Exceptions;

namespace Main.Api.GraphQl.Types.Producer;

[GraphQLName("Producer")]
public record GqlProducer
{
    private readonly ProducerDto? _producer;

    [GraphQLName("id")]
    [Shareable]
    public int Id { get; }

    [GraphQLName("name")]
    public async Task<string> GetNameAsync(
        IProducerByIdDataLoader loader,
        CancellationToken cancellationToken)
        => (await GetProducerAsync(loader, cancellationToken)).Name;

    [GraphQLName("description")]
    public async Task<string?> GetDescriptionAsync(
        IProducerByIdDataLoader loader,
        CancellationToken cancellationToken)
        => (await GetProducerAsync(loader, cancellationToken)).Description;

    [GraphQLName("aliases")]
    public async Task<IReadOnlyCollection<string>> GetAliasesAsync(
        IProducerAliasesByIdDataLoader loader,
        CancellationToken cancellationToken)
        => await loader.LoadAsync(Id, cancellationToken) ?? [];

    [GraphQLName("supplierMappings")]
    public async Task<List<GqlProducerSupplierMapping>> GetSupplierMappingsAsync(
        IProducerSupplierMappingsByIdDataLoader loader,
        CancellationToken cancellationToken)
        => (await loader.LoadAsync(Id, cancellationToken) ?? [])
            .Select(x => new GqlProducerSupplierMapping(x))
            .ToList();

    private async Task<ProducerDto> GetProducerAsync(
        IProducerByIdDataLoader loader,
        CancellationToken cancellationToken)
        => _producer
           ?? await loader.LoadAsync(Id, cancellationToken)
           ?? throw new ProducerNotFoundException(Id);

    public GqlProducer(int id)
    {
        Id = id;
    }

    public GqlProducer(ProducerDto producer) : this(producer.Id)
    {
        _producer = producer;
    }
}
