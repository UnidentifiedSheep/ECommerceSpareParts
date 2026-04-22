using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Articles;
using Main.Abstractions.Dtos.Services.Articles;
using Main.Entities.Product;

namespace Main.Application.Handlers.Articles.CreateArticles;

[AutoSave]
[Transactional]
public record CreateProductsCommand(List<CreateProductDto> NewProducts) : ICommand<CreateProductsResult>;

public record CreateProductsResult(List<int> CreatedIds);

public class CreateProductsHandler(
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope)
    : ICommandHandler<CreateProductsCommand, CreateProductsResult>
{
    public async Task<CreateProductsResult> Handle(CreateProductsCommand request, CancellationToken cancellationToken)
    {
        var products = new List<Product>();

        foreach (var @new in request.NewProducts)
        {
            var product = Product.Create(@new.Sku, @new.Name, @new.ProducerId, @new.Description);
            product.SetIndicator(@new.Indicator);
            product.SetCategory(@new.CategoryId);
            products.Add(product);
        }
        
        await unitOfWork.AddRangeAsync(products, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await PublishEvent(products, cancellationToken);

        return new CreateProductsResult(products.Select(x => x.Id).ToList());
    }

    private async Task PublishEvent(List<Product> products, CancellationToken cancellationToken)
    {
        foreach (var product in products)
            integrationEventScope.Add(new ProductCreatedEvent
            {
                Id = product.Id,
            });
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}