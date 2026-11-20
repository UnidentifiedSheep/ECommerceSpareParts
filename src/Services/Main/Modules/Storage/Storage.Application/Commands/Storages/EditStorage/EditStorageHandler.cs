using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Storage.Exceptions;
using Main.Modules.Storage.Application.Dtos.Storage;
using MediatR;

namespace Main.Modules.Storage.Application.Commands.Storages.EditStorage;

[AutoSave]
[Transactional]
public record EditStorageCommand(string StorageCode, PatchStorageDto EditStorage) : ICommand;

public class EditStorageHandler(IRepository<Entities.Storage.Storage.Storage, string> repository)
	: ICommandHandler<EditStorageCommand>
{
	public async Task<Unit> Handle(EditStorageCommand request, CancellationToken cancellationToken)
	{
		var criteria = Criteria<Entities.Storage.Storage.Storage>
			.New()
			.Where(x => x.Code == request.StorageCode)
			.Track()
			.Build();

		var storage = await repository.FirstOrDefaultAsync(criteria, cancellationToken) ??
			throw new StorageNotFoundException(request.StorageCode);

		var patch = request.EditStorage;

		patch.Location.Apply(storage.SetLocation);
		patch.Description.Apply(storage.SetDescription);
		patch.Type.Apply(storage.SetType);

		return Unit.Value;
	}
}
