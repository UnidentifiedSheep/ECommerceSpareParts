using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Producer.SupplierMappings;
using Main.Entities.Exceptions;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProducerSupplierMappings.CreateProducerSupplierMapping;

[AutoSave]
[Diagnostics]
[Transactional(retryCount: 2, retryDelayMs: 20)]
public record CreateProducerSupplierMappingCommand(
    NewProducerSupplierMapping ProducerSupplierMapping
    ) : ICommand<CreateProducerSupplierMappingResult>;
public record CreateProducerSupplierMappingResult(
    ProducerSupplierMappingDto ProducerSupplierMapping);

public class CreateProducerSupplierMappingHandler(
    IUnitOfWork unitOfWork,
    IReadRepository<ProducerSupplierMapping, int> repository
    ) : ICommandHandler<CreateProducerSupplierMappingCommand, CreateProducerSupplierMappingResult>
{
    public async Task<CreateProducerSupplierMappingResult> Handle(
        CreateProducerSupplierMappingCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await repository.Query
            .AnyAsync(
                x => x.ProducerId == request.ProducerSupplierMapping.ProducerId
                     && x.Supplier == request.ProducerSupplierMapping.Supplier,
                cancellationToken);
        if (exists)
            throw new ProducersSupplierMappingAlreadyExistsException(
                request.ProducerSupplierMapping.ProducerId,
                request.ProducerSupplierMapping.Supplier);

        var model = ProducerSupplierMapping.Create(
            request.ProducerSupplierMapping.ProducerId,
            request.ProducerSupplierMapping.SupplierProducerName.Trim(),
            request.ProducerSupplierMapping.Supplier);

        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateProducerSupplierMappingResult(
            new ProducerSupplierMappingDto
            {
                Id = model.Id,
                ProducerId = model.ProducerId,
                Supplier = model.Supplier,
                SupplierProducerName = model.SupplierProducerName
            });
    }
}
