using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Entities;
using Main.Entities.Storage;
using Main.Enums;
using Mapster;

namespace Main.Application.Handlers.Storages.CreateStorage;

[AutoSave]
[Transactional]
public record CreateStorageCommand(string Name, string? Description, string? Location, StorageType Type)
    : ICommand<CreateStorageResult>;

public record CreateStorageResult(string Name);

public class CreateStorageHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateStorageCommand, CreateStorageResult>
{
    public async Task<CreateStorageResult> Handle(CreateStorageCommand request, CancellationToken cancellationToken)
    {
        var storage = Storage.Create(request.Name, request.Type);
        storage.SetDescription(request.Description);
        storage.SetLocation(request.Location);
        
        await unitOfWork.AddAsync(storage, cancellationToken);
        return new CreateStorageResult(storage.Name);
    }
}