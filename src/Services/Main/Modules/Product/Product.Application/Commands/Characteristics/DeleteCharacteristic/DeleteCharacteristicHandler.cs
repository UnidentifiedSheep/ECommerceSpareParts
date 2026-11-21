using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Product.Exceptions;
using Main.Entities.Product.Product;
using MediatR;

namespace Main.Modules.Product.Application.Commands.Characteristics.DeleteCharacteristic;

[AutoSave]
[Transactional]
public record DeleteCharacteristicCommand(int Id) : ICommand;

public class DeleteCharacteristicHandler(
	IRepository<Characteristic, int> repository,
	IUnitOfWork unitOfWork) : ICommandHandler<DeleteCharacteristicCommand>
{
	public async Task<Unit> Handle(DeleteCharacteristicCommand request, CancellationToken cancellationToken)
	{
		var characteristic = await repository.GetById(request.Id, cancellationToken) ??
			throw new CharacteristicNotFoundException(request.Id);

		unitOfWork.Remove(characteristic);
		return Unit.Value;
	}
}
