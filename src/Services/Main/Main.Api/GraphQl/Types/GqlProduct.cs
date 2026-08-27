using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Application.Dtos.Product;

namespace Main.Api.GraphQl.Types;

[GraphQLName("Product")]
public record GqlProduct(
    [property: GraphQLIgnore]
    ProductDto Product)
{
    [GraphQLName("id")]
    [Shareable]
    public int Id => Product.Id;

    [GraphQLName("sku")]
    public string Sku => Product.Sku;

    [GraphQLName("name")]
    public string Name => Product.Name;

    [GraphQLName("description")]
    public string? Description => Product.Description;
    
    [GraphQLName("indicator")]
    public string? Indicator => Product.Indicator;

    [GraphQLName("images")]
    public List<string> Images => Product.Images;

    [GraphQLName("stock")]
    public int Stock => Product.Stock;

    [GraphQLName("producer")]
    public async Task<GqlProducer> GetProducerAsync(
        ProducerByIdDataLoader producerById,
        CancellationToken cancellationToken)
    {
        var producer = await producerById.LoadAsync(
            Product.ProducerId,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Producer with id {Product.ProducerId} does not exist");

        return producer;
    }
}