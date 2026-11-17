using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Product.Exceptions;
using Main.Entities.Product.Product;
using MediatR;

namespace Main.Modules.Product.Application.Products.Commands;

[AutoSave]
[Transactional]
public record DeleteArticleSizesCommand(int ProductId) : ICommand;

public class DeleteProductSizesHandler(IRepository<ProductSize, int> repository, IUnitOfWork unitOfWork)
	: ICommandHandler<DeleteArticleSizesCommand>
{
	public async Task<Unit> Handle(DeleteArticleSizesCommand request, CancellationToken cancellationToken)
	{
		var sizes = await repository.GetById(request.ProductId, cancellationToken) ??
			throw new ProductSizesNotFoundException(request.ProductId);

		unitOfWork.Remove(sizes);
		return Unit.Value;
	}
}
