using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Exceptions;
using Main.Entities.Product;
using Main.Entities.Product.Product.ValueObjects;
using Main.Modules.Product.Application.Dtos.Product;
using Main.Modules.Product.Application.Interfaces.Persistence;

namespace Main.Modules.Product.Application.Commands.Products.CreateProducts;

[AutoSave]
[Transactional]
public record CreateProductsCommand(List<CreateProductDto> NewProducts) : ICommand<CreateProductsResult>;

public record CreateProductsResult(List<int> CreatedIds);

public class CreateProductsHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
	: ICommandHandler<CreateProductsCommand, CreateProductsResult>
{
	public async Task<CreateProductsResult> Handle(
		CreateProductsCommand request,
		CancellationToken cancellationToken)
	{
		var keys = request.NewProducts.Select(GetProductKey).ToList();
		if (keys.Distinct().Count() != keys.Count)
			throw new InvalidInputException(ArticleCreateArticlesDuplicateMessage.Instance);

		var existingKeys = await productRepository.GetExistingProductKeys(keys, cancellationToken);
		if (existingKeys.Count > 0)
			throw new InvalidInputException(ArticleCreateArticlesDuplicateMessage.Instance);

		var products = new List<Entities.Product.Product.Product>();

		foreach (var @new in request.NewProducts)
		{
			var product = Entities.Product.Product.Product.Create(
				@new.Sku,
				@new.Name,
				@new.ProducerId,
				@new.Description);
			product.SetIndicator(@new.Indicator);
			product.SetProductGroup(@new.GroupId);
			products.Add(product);
		}

		await unitOfWork.AddRangeAsync(products, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new CreateProductsResult(products.Select(x => x.Id).ToList());
	}

	private static (string NormalizedSku, int ProducerId) GetProductKey(CreateProductDto product)
		=> (new Sku(product.Sku).NormalizedValue, product.ProducerId);
}
