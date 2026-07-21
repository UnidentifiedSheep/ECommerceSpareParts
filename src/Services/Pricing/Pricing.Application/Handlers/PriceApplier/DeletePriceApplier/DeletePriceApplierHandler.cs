using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Attributes;
using MediatR;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;
using Pricing.Entities.Exceptions;

namespace Pricing.Application.Handlers.PriceApplier.DeletePriceApplier;

[Diagnostics]
[Transactional, AutoSave]
public record DeletePriceApplierCommand(string SystemName) : ICommand;

public class DeletePriceApplierHandler(
    INamedObjectRegistry<ApplierNamedObjectBase> registry,
    IRepository<Entities.Pricing.PriceApplier, string> repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePriceApplierCommand>
{
    public async Task<Unit> Handle(
        DeletePriceApplierCommand request,
        CancellationToken cancellationToken)
    {
        if (registry.TryGetBySystemName(request.SystemName) is not null)
            throw new LocalPriceApplierCannotBeDeletedException(request.SystemName);

        var applier = await repository.GetById(
                          request.SystemName,
                          cancellationToken)
                      ?? throw new PriceApplierNotFoundException(request.SystemName);

        unitOfWork.Remove(applier);
        return Unit.Value;
    }
}
