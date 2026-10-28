using System.Data;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.Extensions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Main.Entities.Product.Enrichment;
using MediatR;

namespace Main.Application.Handlers.Products;

[Transactional(IsolationLevel.Serializable, 30, 3), AutoSave]
public record DeleteProductCommand(int ProductId, uint RowVersion) : ICommand;

public class DeleteProductHandler(
	IProductRepository products,
	IRepository<CatalogueCandidate, Guid> candidates,
	IUnitOfWork unitOfWork) : ICommandHandler<DeleteProductCommand>
{
	public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
	{
		var id = request.ProductId;
		var product = await products.FirstOrDefaultAsync(
			Criteria<Product>.New()
				.Where(item => item.Id == id)
				.Include(item => item.Images)
				.Track()
				.ForUpdate()
				.Build(), cancellationToken)
			?? throw new ProductNotFoundException(id);

		product.ValidateVersion(request.RowVersion);

		if (await products.GetDeletionBlockerAsync(id, cancellationToken) is { } blocker)
			throw new ProductDeletionNotAllowedException(blocker);

		var candidate = await candidates.FirstOrDefaultAsync(
			Criteria<CatalogueCandidate>.New()
				.Where(item => item.ProductId == id)
				.Track()
				.Build(),
			cancellationToken);

		candidate?.RemoveProductMapping();

		unitOfWork.Remove(product);
		return Unit.Value;
	}
}
