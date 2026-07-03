using Abstractions.Interfaces.Exceptions;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Producer;
using Localization.Abstractions.Interfaces;
using Main.Application.Dtos.Producer;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Producer;

namespace Main.Application.Handlers.Producers;

[AutoSave]
[Transactional(
    retryErrors: ["23505"],
    retryCount: 2,
    retryDelayMs: 20)]
public record CreateProducerBatchCommand(
    IEnumerable<NewProducerDto> NewProducers
) : ICommand<CreateProducerBatchResult>;

public record CreateProducerBatchResult(
    int Created,
    int Skipped,
    IReadOnlyList<CreateProducerBatchError> Errors
);

public record CreateProducerBatchError(
    int Index,
    string Message
);

public class CreateProducerBatchHandler(
    IProducerRepository producerRepository,
    IScopedStringLocalizer stringLocalizer,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateProducerBatchCommand, CreateProducerBatchResult>
{
    public async Task<CreateProducerBatchResult> Handle(
        CreateProducerBatchCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<CreateProducerBatchError>();
        var producers = new List<Producer>();
        var uniqNames = new HashSet<string>();

        var idx = 0;
        foreach (var dto in request.NewProducers)
        {
            var currentIdx = idx++;
            var pr = ProcessDto(
                currentIdx,
                dto,
                errors);

            if (pr == null) continue;
            if (!uniqNames.Add(pr.Name))
            {
                errors.Add(
                    new CreateProducerBatchError(
                        currentIdx,
                        stringLocalizer.Get("producer.duplicate.name.in.batch")));
                continue;
            }

            producers.Add(pr);
        }

        var existingProducers =
            await GetExistingProducers(uniqNames, cancellationToken);

        var toAdd = producers
            .Where(x => !existingProducers.ContainsKey(x.Name))
            .ToList();

        await unitOfWork.AddRangeAsync(toAdd, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateProducerBatchResult(
            toAdd.Count,
            producers.Count - toAdd.Count,
            errors);
    }

    private Producer? ProcessDto(
        int idx,
        NewProducerDto dto,
        List<CreateProducerBatchError> errors)
    {
        try { return Producer.Create(dto.Name, dto.Description); }
        catch (Exception ex)
        {
            var message = ex is ILocalizableException localizableException
                ? stringLocalizer.GetOrDefault(localizableException.MessageKey) ?? ex.Message
                : ex.Message;

            errors.Add(new CreateProducerBatchError(idx, message));

            return null;
        }
    }

    private async Task<Dictionary<string, Producer>> GetExistingProducers(
        IEnumerable<string> names,
        CancellationToken cancellationToken)
    {
        var criteria = Criteria<Producer>.New()
            .Where(x => names.Contains(x.Name))
            .Track(false)
            .Build();

        return (await producerRepository.ListAsync(criteria, cancellationToken))
            .ToDictionary(x => x.Name);
    }
}