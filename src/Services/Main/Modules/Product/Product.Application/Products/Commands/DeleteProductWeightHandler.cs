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
public record DeleteProductWeightCommand(int ProductId) : ICommand;

public class DeleteProductWeightHandler(
	IRepository<ProductWeight, int> repository,
	IUnitOfWork unitOfWork) : ICommandHandler<DeleteProductWeightCommand>
{
	public async Task<Unit> Handle(DeleteProductWeightCommand request, CancellationToken cancellationToken)
	{
		var weight = await repository.GetById(request.ProductId, cancellationToken) ??
			throw new ProductWeightNotFoundException(request.ProductId);
		unitOfWork.Remove(weight);
		return Unit.Value;
	}
}
