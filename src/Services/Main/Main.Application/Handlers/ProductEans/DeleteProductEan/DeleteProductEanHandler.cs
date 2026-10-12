using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ProductEans.DeleteProductEan;

[AutoSave]
[Transactional]
public record DeleteProductEanCommand(int ProductId, string Ean) : ICommand;

public class DeleteProductEanHandler(
	IRepository<ProductEan, ProductEanKey> repository,
	IUnitOfWork unitOfWork) : ICommandHandler<DeleteProductEanCommand>
{
	public async Task<Unit> Handle(DeleteProductEanCommand request, CancellationToken cancellationToken)
	{
		var ean = request.Ean.Trim();
		var productEan = await repository.GetById(
			new ProductEanKey(request.ProductId, ean),
			cancellationToken) ?? throw new ProductEanNotFoundException(request.ProductId, ean);

		unitOfWork.Remove(productEan);
		return Unit.Value;
	}
}
