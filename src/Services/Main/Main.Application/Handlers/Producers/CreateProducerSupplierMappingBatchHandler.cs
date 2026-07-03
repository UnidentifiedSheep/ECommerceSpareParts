using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Enums;
using MassTransit;

namespace Main.Application.Handlers.Producers;

[AutoSave]
[Diagnostics]
[Transactional(retryErrors: ["23505"], retryCount: 2, retryDelayMs: 20)]
public record CreateProducerSupplierMappingBatchCommand(
    IEnumerable<CreateProducerSupplierMappingBatchItem> Items
    ) : ICommand<CreateProducerSupplierMappingBatchResult>;

public record CreateProducerSupplierMappingBatchItem(
    int ProducerId, 
    Supplier Supplier, 
    string ProducerName);

public record CreateProducerSupplierMappingBatchResult(
    int Created,
    int Skipped,
    IReadOnlyList<CreateProducerAliasesBatchError> Errors
);

public record CreateProducerSupplierMappingBatchError(int Index, string Message);

public class CreateProducerSupplierMappingBatchHandler(
    ) : ICommandHandler<CreateProducerSupplierMappingBatchCommand, CreateProducerSupplierMappingBatchResult>
{
    public Task<CreateProducerSupplierMappingBatchResult> Handle(
        CreateProducerSupplierMappingBatchCommand request, 
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}