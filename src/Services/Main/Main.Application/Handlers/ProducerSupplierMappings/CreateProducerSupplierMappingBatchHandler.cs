using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.Extensions;
using Enums;
using Localization.Abstractions.Interfaces;
using Main.Application.Dtos.Producer.SupplierMappings;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Producer;

namespace Main.Application.Handlers.ProducerSupplierMappings;

[AutoSave]
[Diagnostics]
[Transactional(retryErrors: ["23505"], retryCount: 2, retryDelayMs: 20)]
public record CreateProducerSupplierMappingBatchCommand(
    IEnumerable<NewProducerSupplierMapping> Items
) : ICommand<CreateProducerSupplierMappingBatchResult>;

public record CreateProducerSupplierMappingBatchResult(
    int Created,
    int Skipped,
    IReadOnlyList<CreateProducerSupplierMappingBatchError> Errors
);

public record CreateProducerSupplierMappingBatchError(int Index, string Message);

internal record ProcessedProducerSupplierMappingBatchItem(
    int Index,
    int ProducerId,
    string SupplierProducerName,
    Supplier Supplier
);

public class CreateProducerSupplierMappingBatchHandler(
    IProducerRepository producerRepository,
    IScopedStringLocalizer localizer
) : ICommandHandler<CreateProducerSupplierMappingBatchCommand, CreateProducerSupplierMappingBatchResult>
{
    public async Task<CreateProducerSupplierMappingBatchResult> Handle(
        CreateProducerSupplierMappingBatchCommand request,
        CancellationToken cancellationToken)
    {
        var items = request.Items.ToList();
        if (items.Count == 0)
            return new CreateProducerSupplierMappingBatchResult(
                0,
                0,
                []);

        var processedItems = new List<ProcessedProducerSupplierMappingBatchItem>();
        var errors = new List<CreateProducerSupplierMappingBatchError>();
        var uniqMappings = new HashSet<(int ProducerId, Supplier Supplier)>();

        var idx = 0;
        foreach (var item in items)
        {
            var currentIdx = idx++;
            var processed = ProcessDto(
                currentIdx,
                item,
                errors);
            if (processed is null) continue;

            if (!uniqMappings.Add((processed.ProducerId, processed.Supplier)))
            {
                errors.Add(
                    new CreateProducerSupplierMappingBatchError(
                        currentIdx,
                        localizer.Get("producer.supplier.mapping.duplicate.in.batch")));
                continue;
            }

            processedItems.Add(processed);
        }

        var producerIds = processedItems
            .Select(x => x.ProducerId)
            .Distinct()
            .ToList();

        var existingProducerIds = (await producerRepository.ListAsync(
                Criteria<Producer>.New()
                    .Where(x => producerIds.Contains(x.Id))
                    .Track(false)
                    .Build(),
                cancellationToken))
            .Select(x => x.Id)
            .ToHashSet();

        var toAdd = new List<ProducerSupplierMapping>();
        foreach (var item in processedItems)
        {
            if (!existingProducerIds.Contains(item.ProducerId))
            {
                errors.Add(
                    new CreateProducerSupplierMappingBatchError(
                        item.Index,
                        localizer.Get("producer.supplier.mapping.producer.not.found.in.batch")));
                continue;
            }

            toAdd.Add(
                ProducerSupplierMapping.Create(
                    item.ProducerId,
                    item.SupplierProducerName,
                    item.Supplier));
        }

        await producerRepository.AddSupplierMappingsOnConflictDoNothingAsync(
            toAdd,
            cancellationToken);

        return new CreateProducerSupplierMappingBatchResult(
            toAdd.Count,
            processedItems.Count - toAdd.Count,
            errors);
    }

    private ProcessedProducerSupplierMappingBatchItem? ProcessDto(
        int idx,
        NewProducerSupplierMapping dto,
        List<CreateProducerSupplierMappingBatchError> errors)
    {
        var supplierProducerName = dto.SupplierProducerName
            .TrimSafe();

        if (string.IsNullOrWhiteSpace(supplierProducerName))
        {
            errors.Add(
                new CreateProducerSupplierMappingBatchError(
                    idx,
                    localizer.Get("producer.supplier.mapping.supplier.producer.name.required")));
            return null;
        }

        return new ProcessedProducerSupplierMappingBatchItem(
            idx,
            dto.ProducerId,
            supplierProducerName,
            dto.Supplier);
    }
}
