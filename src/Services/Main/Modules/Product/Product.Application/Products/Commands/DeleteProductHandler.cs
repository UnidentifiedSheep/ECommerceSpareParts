using System.Data;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.Extensions;
using Main.Entities.Product.Exceptions;
using Main.Entities.Product.Product.Enrichment;
using Main.Modules.Product.Application.Interfaces.Persistence;
using Main.Modules.Product.Application.Interfaces.Services;
using MediatR;

namespace Main.Modules.Product.Application.Products.Commands;

[Transactional(
	IsolationLevel.Serializable,
	30,
	3)]
[AutoSave]
public record DeleteProductCommand(int ProductId, uint RowVersion) : ICommand;

public class DeleteProductHandler(
	IProductRepository products,
	IProductDeletionBlockerResolver deletionBlockers,
	IRepository<CatalogueCandidate, Guid> candidates,
	IUnitOfWork unitOfWork) : ICommandHandler<DeleteProductCommand>
{
	public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
	{
		var id = request.ProductId;
		var product = await products.FirstOrDefaultAsync(
				Criteria<Entities.Product.Product.Product>.New()
					.Where(item => item.Id == id)
					.Include(item => item.Images)
					.Track()
					.ForUpdate()
					.Build(),
				cancellationToken)
			?? throw new ProductNotFoundException(id);

		product.ValidateVersion(request.RowVersion);

		if (await deletionBlockers.GetAsync(id, cancellationToken) is { } blocker)
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
