using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Characteristics.DeleteCharacteristic;

[AutoSave]
[Transactional]
public record DeleteCharacteristicCommand(int Id) : ICommand;

public class DeleteCharacteristicHandler(
	IRepository<Characteristic, int> repository,
	IReadRepository<ProductCharacteristic, (int, int)> productCharacteristics,
	IUnitOfWork unitOfWork) : ICommandHandler<DeleteCharacteristicCommand>
{
	public async Task<Unit> Handle(DeleteCharacteristicCommand request, CancellationToken cancellationToken)
	{
		var characteristic = await repository.GetById(request.Id, cancellationToken) ??
			throw new CharacteristicNotFoundException(request.Id);

		if (await productCharacteristics.Query.AnyAsync(x => x.CharacteristicId == request.Id, cancellationToken))
			throw new CharacteristicInUseException(request.Id);

		unitOfWork.Remove(characteristic);
		return Unit.Value;
	}
}
