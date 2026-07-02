using Abstractions.Interfaces.Exceptions;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Producer;
using Localization.Abstractions.Interfaces;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Producer;

namespace Main.Application.Handlers.Producers;

[AutoSave]
[Diagnostics]
[Transactional(
    retryErrors: ["23505"],
    retryCount: 2,
    retryDelayMs: 20)]
public record CreateProducerOtherNamesBatchCommand(
    IEnumerable<CreateProducerOtherNamesBatchItem> Items
) : ICommand<CreateProducerOtherNamesBatchResult>;

public record CreateProducerOtherNamesBatchResult(
    int Created,
    int Skipped,
    IReadOnlyList<CreateProducerOtherNamesBatchError> Errors
);

public record CreateProducerOtherNamesBatchItem(
    string OriginalName,
    string Alias
);

public record CreateProducerOtherNamesBatchError(int Index, string Message);

internal record ProcessedProducerOtherNameBatchItem(
    int Index,
    string OriginalName,
    string Alias
);

public class CreateProducerOtherNamesBatchHandler(
    IRepository<ProducerAlias, string> aliasRepository,
    IScopedStringLocalizer localizer,
    IProducerRepository producerRepository,
    IIntegrationEventScope integrationEventScope,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateProducerOtherNamesBatchCommand, CreateProducerOtherNamesBatchResult>
{
    public async Task<CreateProducerOtherNamesBatchResult> Handle(
        CreateProducerOtherNamesBatchCommand request,
        CancellationToken cancellationToken)
    {
        var items = request.Items.ToList();
        if (items.Count == 0)
            return new CreateProducerOtherNamesBatchResult(
                0,
                0,
                []);

        var processedItems = new List<ProcessedProducerOtherNameBatchItem>();
        var toAdd = new List<ProducerAlias>();
        var errors = new List<CreateProducerOtherNamesBatchError>();
        var uniqOtherNames = new HashSet<string>();

        var idx = 0;
        foreach (var item in items)
        {
            var currentIdx = idx++;
            var processed = ProcessDto(
                currentIdx,
                item,
                errors);
            if (processed is null) continue;

            if (!uniqOtherNames.Add(processed.Alias))
            {
                errors.Add(
                    new CreateProducerOtherNamesBatchError(
                        currentIdx,
                        localizer.Get("producer.other.name.duplicate.in.batch")));
                continue;
            }

            processedItems.Add(processed);
        }

        var existingAliases = await GetOtherNames(processedItems, cancellationToken);
        var existingProducers = await GetProducers(processedItems, cancellationToken);

        foreach (var item in processedItems)
        {
            if (existingAliases.ContainsKey(item.Alias))
            {
                errors.Add(
                    new CreateProducerOtherNamesBatchError(
                        item.Index,
                        localizer.Get("producer.other.name.already.taken")));
                continue;
            }

            if (!existingProducers.TryGetValue(item.OriginalName, out var producer))
            {
                errors.Add(
                    new CreateProducerOtherNamesBatchError(
                        item.Index,
                        localizer.Get("producer.other.name.producer.not.found.in.batch")));
                continue;
            }

            toAdd.Add(
                ProducerAlias.Create(
                    producer.Id,
                    item.Alias));
        }

        await unitOfWork.AddRangeAsync(toAdd, cancellationToken);
        foreach (var producerId in toAdd.Select(x => x.ProducerId).Distinct())
            integrationEventScope.Add(
                new ProducerUpdatedEvent
                {
                    Id = producerId
                });

        return new CreateProducerOtherNamesBatchResult(
            toAdd.Count,
            processedItems.Count - toAdd.Count,
            errors);
    }

    private ProcessedProducerOtherNameBatchItem? ProcessDto(
        int idx,
        CreateProducerOtherNamesBatchItem dto,
        List<CreateProducerOtherNamesBatchError> errors)
    {
        try
        {
            var producerOtherName = ProducerAlias.Create(
                0,
                dto.Alias);

            return new ProcessedProducerOtherNameBatchItem(
                idx,
                Producer.ToNormalizedName(dto.OriginalName),
                producerOtherName.Alias);
        }
        catch (Exception ex)
        {
            var message = ex is ILocalizableException localizableException
                ? localizer.GetOrDefault(
                    localizableException.MessageKey,
                    localizableException.Arguments ?? []) ?? ex.Message
                : ex.Message;

            errors.Add(new CreateProducerOtherNamesBatchError(idx, message));

            return null;
        }
    }

    private async Task<Dictionary<string, ProducerAlias>> GetOtherNames(
        List<ProcessedProducerOtherNameBatchItem> items,
        CancellationToken cancellationToken)
    {
        var aliases = items
            .Select(x => x.Alias)
            .Distinct()
            .ToList();

        return (await aliasRepository.ListAsync(
            Criteria<ProducerAlias>.New()
                .Where(x => aliases.Contains(x.Alias))
                .Track(false)
                .Build(),
            cancellationToken
        )).ToDictionary(x => x.Alias);
    }

    private async Task<Dictionary<string, Producer>> GetProducers(
        List<ProcessedProducerOtherNameBatchItem> items,
        CancellationToken cancellationToken)
    {
        var producerNames = items
            .Select(x => x.OriginalName)
            .Distinct()
            .ToList();
        return (await producerRepository.ListAsync(
            Criteria<Producer>.New()
                .Where(x => producerNames.Contains(x.Name))
                .Track(false)
                .Build(),
            cancellationToken
        )).ToDictionary(x => x.Name);
    }
}