using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Producer;
using MediatR;

namespace Main.Application.Handlers.ProducerSupplierMappings;

[Diagnostics]
[Transactional, AutoSave]
public record DeleteProducerSupplierMappingCommand(int Id) : ICommand;

public class DeleteProducerSupplierMappingHandler(
    IRepository<ProducerSupplierMapping, int> repository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteProducerSupplierMappingCommand>
{
    public async Task<Unit> Handle(DeleteProducerSupplierMappingCommand request, CancellationToken cancellationToken)
    {
        var mapping = await repository.GetById(request.Id, cancellationToken)
                      ?? throw new ProducersSupplierMappingNotFoundException(request.Id);
        unitOfWork.Remove(mapping);
        return Unit.Value;
    }
}