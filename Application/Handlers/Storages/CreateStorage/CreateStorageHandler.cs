using Application.Interfaces;
using Core.Attributes;
using Core.Entities;
using Core.Exceptions.Storages;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Mapster;
using MediatR;

namespace Application.Handlers.Storages.CreateStorage;

[Transactional]
public record CreateStorageCommand(string Name, string? Description, string? Location) : ICommand;

public class CreateStorageHandler(IStoragesRepository repository, 
    IUnitOfWork unitOfWork) : ICommandHandler<CreateStorageCommand>
{
    public async Task<Unit> Handle(CreateStorageCommand request, CancellationToken cancellationToken)
    {
        await ValidateData(request.Name, cancellationToken);
        var newStorage = request.Adapt<Storage>();
        await unitOfWork.AddAsync(newStorage, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task ValidateData(string name, CancellationToken cancellationToken = default)
    {
        if (await repository.StorageExistsAsync(name, cancellationToken)) 
            throw new StorageNameIsTakenException(name);
    }
}