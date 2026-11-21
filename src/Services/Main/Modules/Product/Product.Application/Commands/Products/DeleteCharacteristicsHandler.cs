using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Product.Exceptions;
using Main.Entities.Product.Product;
using MediatR;

namespace Main.Modules.Product.Application.Commands.Products;

[AutoSave]
[Transactional]
public record DeleteCharacteristicsCommand(int ProductId, int CharacteristicId) : ICommand;

public class DeleteCharacteristicsHandler(
	IRepository<ProductCharacteristic, (int, int)> repository,
	IUnitOfWork unitOfWork) : ICommandHandler<DeleteCharacteristicsCommand>
{
	public async Task<Unit> Handle(DeleteCharacteristicsCommand request, CancellationToken cancellationToken)
	{
		var entity = await repository.GetById(
				(request.ProductId, request.CharacteristicId),
				cancellationToken) ??
			throw new ProductCharacteristicsNotFoundException(request.ProductId, request.CharacteristicId);
		unitOfWork.Remove(entity);
		return Unit.Value;
	}
}
