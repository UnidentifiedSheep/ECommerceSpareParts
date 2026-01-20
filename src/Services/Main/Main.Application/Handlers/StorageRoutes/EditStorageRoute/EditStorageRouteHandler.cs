using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.StorageRoutes;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.StorageRoutes.EditStorageRoute;

[Transactional]
public record EditStorageRouteCommand(Guid Id, PatchStorageRouteDto PatchStorageRoute) : ICommand;

public class EditStorageRouteHandler(IStorageRoutesRepository storageRoutesRepository, IUnitOfWork unitOfWork) 
    : ICommandHandler<EditStorageRouteCommand>
{
    public async Task<Unit> Handle(EditStorageRouteCommand request, CancellationToken cancellationToken)
    {
        StorageRoute storageRoute = await storageRoutesRepository
            .GetStorageRouteAsync(request.Id, true, cancellationToken) ?? throw new StorageRouteNotFound(request.Id);

        request.PatchStorageRoute.Adapt(storageRoute);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}