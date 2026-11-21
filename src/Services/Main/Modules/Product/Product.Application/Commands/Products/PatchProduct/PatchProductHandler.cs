using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Entities.Product.Exceptions;
using Main.Modules.Product.Application.Dtos.Product;
using Main.Modules.Product.Application.Interfaces.Persistence;
using MediatR;

namespace Main.Modules.Product.Application.Commands.Products.PatchProduct;

[Transactional]
[AutoSave]
public record PatchProductCommand(int ProductId, PatchProductDto PatchProduct) : ICommand;

public class PatchProductHandler(IProductRepository productRepository) : ICommandHandler<PatchProductCommand>
{
	public async Task<Unit> Handle(PatchProductCommand request, CancellationToken cancellationToken)
	{
		var patch = request.PatchProduct;
		var product = await productRepository.GetById(request.ProductId, cancellationToken) ??
			throw new ProductNotFoundException(request.ProductId);

		patch.Description.Apply(product.SetDescription);
		patch.GroupId.Apply(product.SetProductGroup);
		patch.Sku.Apply(x => product.SetSku(x));
		patch.ProducerId.Apply(product.SetProducerId);
		patch.Indicator.Apply(x => product.SetIndicator(x));
		patch.PackingUnit.Apply(product.SetPackingUnit);
		patch.PairId.Apply(product.SetPair);
		patch.Name.Apply(x => product.SetName(x));

		return Unit.Value;
	}
}
