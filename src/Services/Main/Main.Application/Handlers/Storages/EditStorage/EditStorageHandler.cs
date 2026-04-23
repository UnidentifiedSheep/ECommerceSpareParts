using Abstractions.Interfaces.Services;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Exceptions.Storages;
using Main.Application.Dtos.Storage;
using Main.Entities.Storage;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Storages.EditStorage;

[AutoSave]
[Transactional]
public record EditStorageCommand(string StorageName, PatchStorageDto EditStorage) : ICommand;

public class EditStorageHandler(IRepository<Storage, string> repository)
    : ICommandHandler<EditStorageCommand>
{
    public async Task<Unit> Handle(EditStorageCommand request, CancellationToken cancellationToken)
    {
        var criteria = Criteria<Storage>.New()
            .Where(x => x.Name == request.StorageName)
            .Include(x => x.Owners)
            .Track()
            .Build();
        
        var storage = await repository.FirstOrDefaultAsync(criteria, cancellationToken)
                      ?? throw new StorageNotFoundException(request.StorageName);

        var patch = request.EditStorage;
        
        patch.Location.Apply(storage.SetLocation);
        patch.Description.Apply(storage.SetDescription);
        patch.Type.Apply(storage.SetType);
        
        return Unit.Value;
    }
}