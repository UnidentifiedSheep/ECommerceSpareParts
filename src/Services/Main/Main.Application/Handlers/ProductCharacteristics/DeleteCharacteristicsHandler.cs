using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ProductCharacteristics;

[AutoSave]
[Transactional]
public record DeleteCharacteristicsCommand(int ProductId, int CharacteristicId) : ICommand;

public class DeleteCharacteristicsHandler(
	IRepository<ProductCharacteristic, (int, int)> repository,
	IUnitOfWork unitOfWork) : ICommandHandler<DeleteCharacteristicsCommand>
{
	public async Task<Unit> Handle(DeleteCharacteristicsCommand request, CancellationToken cancellationToken)
	{
		var entity = await repository.GetById((request.ProductId, request.CharacteristicId), cancellationToken) ??
			throw new ProductCharacteristicsNotFoundException(request.ProductId, request.CharacteristicId);
		unitOfWork.Remove(entity);
		return Unit.Value;
	}
}
