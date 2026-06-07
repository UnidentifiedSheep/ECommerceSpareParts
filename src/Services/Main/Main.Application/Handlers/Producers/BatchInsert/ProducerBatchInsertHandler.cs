using System.Globalization;
using System.Text.Json.Serialization;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Exceptions;
using Application.Common.Interfaces.Cqrs;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Localization.Abstractions.Interfaces;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions;
using Main.Entities.Producer;

namespace Main.Application.Handlers.Producers.BatchInsert;

public record ProducerBatchInsertCommand(string FileName) : ICommand<ProducerBatchInsertResult>;
public record ProducerBatchInsertResult(List<ProducerBatchInsertError> Errors);
public record ProducerBatchInsertError(int RowIdx, string Error);
public record NewProducerCsvDto
{
    [Name("Name")]
    public required string Name { get; init; }
    [Name("Description")]
    public string? Description { get; init; }
}

public class ProducerBatchInsertHandler(
    IS3StorageService s3Service,
    IProducerRepository producerRepository,
    IScopedStringLocalizer stringLocalizer
    ) : ICommandHandler<ProducerBatchInsertCommand, ProducerBatchInsertResult>
{
    private const int BatchSize = 1000;
    private const int MaxErrors = 10_000;
    
    public async Task<ProducerBatchInsertResult> Handle(ProducerBatchInsertCommand request, CancellationToken cancellationToken)
    {
        await using var stream = await s3Service.DownloadFileAsync(
            BucketNames.Uploads, 
            request.FileName, cancellationToken);
        
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        int rowIdx = 1;
        var errors = new List<ProducerBatchInsertError>();
        var producersToAdd = new List<Producer>();
        
        await foreach (var row in csv.GetRecordsAsync<NewProducerCsvDto>(cancellationToken))
        {
            var producer = ProcessRow(rowIdx, row, errors);

            if (errors.Count >= MaxErrors)
                throw new TooManyErrorsWhenProcessingBatch();

            if (producer != null) producersToAdd.Add(producer);

            await InsertAndClear(
                producersToAdd,
                x => x.Count >= BatchSize,
                cancellationToken);
            
            rowIdx++;
        }
        
        await InsertAndClear(
            producersToAdd,
            x => x.Count > 0,
            cancellationToken);
        
        return new ProducerBatchInsertResult(errors);
    }

    private async Task InsertAndClear(
        List<Producer> producers,
        Predicate<List<Producer>> insertWhen,
        CancellationToken cancellationToken = default)
    {
        if (!insertWhen(producers)) return;
        await producerRepository.BulkInsertOnConflictDoNothing(producers, cancellationToken);
        producers.Clear();
    }

    private Producer? ProcessRow(
        int rowIdx,
        NewProducerCsvDto row, 
        List<ProducerBatchInsertError> errors)
    {
        try
        {
            return Producer.Create(row.Name, row.Description);
        }
        catch (Exception ex)
        {
            if (ex is ILocalizableException localizableException)
                errors.Add(new ProducerBatchInsertError(
                    rowIdx, 
                    stringLocalizer.GetOrDefault(localizableException.MessageKey)
                    ?? ex.Message));
            else
                errors.Add(new ProducerBatchInsertError(
                    rowIdx,
                    ex.Message));
            
            return null;
        }
    }
}