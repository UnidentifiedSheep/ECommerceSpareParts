using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Attributes;
using Contracts.Products;
using Exceptions;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;

namespace Main.Application.Handlers.Products.CreateProducts;

[AutoSave]
[Transactional]
public record CreateProductsCommand(
    List<CreateProductDto> NewProducts
) : ICommand<CreateProductsResult>;

public record CreateProductsResult(List<int> CreatedIds);

public class CreateProductsHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<CreateProductsCommand, CreateProductsResult>
{
    public async Task<CreateProductsResult> Handle(
        CreateProductsCommand request,
        CancellationToken cancellationToken)
    {
        var keys = request.NewProducts
            .Select(GetProductKey)
            .ToList();
        if (keys.Distinct().Count() != keys.Count)
            throw new InvalidInputException("article.create.articles.duplicate");

        var existingKeys = await productRepository.GetExistingProductKeys(
            keys,
            cancellationToken);
        if (existingKeys.Count > 0)
            throw new InvalidInputException("article.create.articles.duplicate");

        var products = new List<Product>();

        foreach (var @new in request.NewProducts)
        {
            var product = Product.Create(
                @new.Sku,
                @new.Name,
                @new.ProducerId,
                @new.Description);
            product.SetIndicator(@new.Indicator);
            product.SetCategory(@new.CategoryId);
            products.Add(product);
        }

        await unitOfWork.AddRangeAsync(products, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateProductsResult(products.Select(x => x.Id).ToList());
    }

    private static (string NormalizedSku, int ProducerId) GetProductKey(CreateProductDto product)
        => (new Sku(product.Sku).NormalizedValue, product.ProducerId);
}
