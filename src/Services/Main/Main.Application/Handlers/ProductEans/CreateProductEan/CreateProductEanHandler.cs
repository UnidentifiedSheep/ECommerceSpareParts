using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ProductEans.CreateProductEan;

[AutoSave]
[Transactional]
public record CreateProductEanCommand(int ProductId, string Ean) : ICommand;

public class CreateProductEanHandler(
	IRepository<ProductEan, ProductEanKey> repository,
	IUnitOfWork unitOfWork) : ICommandHandler<CreateProductEanCommand>
{
	public async Task<Unit> Handle(CreateProductEanCommand request, CancellationToken cancellationToken)
	{
		var productEan = ProductEan.Create(request.ProductId, request.Ean);

		if (await repository.GetById(productEan.GetId(), cancellationToken) is not null)
			throw new ProductEanAlreadyExistsException(request.ProductId, productEan.Ean);

		await unitOfWork.AddAsync(productEan, cancellationToken);
		return Unit.Value;
	}
}
