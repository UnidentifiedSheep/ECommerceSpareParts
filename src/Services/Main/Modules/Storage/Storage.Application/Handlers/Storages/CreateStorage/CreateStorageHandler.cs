using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Main.Entities.Storage.Enums;

namespace Main.Modules.Storage.Application.Handlers.Storages.CreateStorage;

[AutoSave]
[Transactional]
public record CreateStorageCommand(
	string Code,
	string? Description,
	string? Location,
	StorageType Type) : ICommand<CreateStorageResult>;

public record CreateStorageResult(string Code);

public class CreateStorageHandler(IUnitOfWork unitOfWork)
	: ICommandHandler<CreateStorageCommand, CreateStorageResult>
{
	public async Task<CreateStorageResult> Handle(
		CreateStorageCommand request,
		CancellationToken cancellationToken)
	{
		var storage = Entities.Storage.Storage.Storage.Create(request.Code, request.Type);
		storage.SetDescription(request.Description);
		storage.SetLocation(request.Location);

		await unitOfWork.AddAsync(storage, cancellationToken);
		return new CreateStorageResult(storage.Code);
	}
}
