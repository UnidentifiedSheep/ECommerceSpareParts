using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using MediatR;

namespace Main.Application.Handlers.Storages.DeleteStorage;

[AutoSave]
[Transactional]
public record DeleteStorageCommand(string StorageCode) : ICommand;

public class DeleteStorageHandler(IRepository<Storage, string> repository, IUnitOfWork unitOfWork)
	: ICommandHandler<DeleteStorageCommand>
{
	public async Task<Unit> Handle(DeleteStorageCommand request, CancellationToken cancellationToken)
	{
		var storage = await repository.GetById(request.StorageCode, cancellationToken) ??
			throw new StorageNotFoundException(request.StorageCode);

		unitOfWork.Remove(storage);
		return Unit.Value;
	}
}
