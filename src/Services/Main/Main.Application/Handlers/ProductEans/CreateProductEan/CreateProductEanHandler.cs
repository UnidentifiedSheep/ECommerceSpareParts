using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ProductEans.CreateProductEan;

[AutoSave]
[Transactional]
public record CreateProductEanCommand(int ProductId, string Ean) : ICommand;

public class CreateProductEanHandler(
	IUnitOfWork unitOfWork) : ICommandHandler<CreateProductEanCommand>
{
	public async Task<Unit> Handle(CreateProductEanCommand request, CancellationToken cancellationToken)
	{
		var productEan = ProductEan.Create(request.ProductId, request.Ean);

		await unitOfWork.AddAsync(productEan, cancellationToken);
		return Unit.Value;
	}
}
