using System.Globalization;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Exceptions;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Producer;

namespace Main.Application.Lrts.ProducerImport;

public class ProducerImportLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    IProducerRepository producerRepository,
    IScopedStringLocalizer stringLocalizer)
    : LrtNamedObjectBase(jobRepository, unitOfWork)
{
    private const int BatchSize = 1000;
    private const int MaxErrors = 10_000;

    public override string SystemName => nameof(ProducerImportLrt);
    public override string NameLocalizationKey => "lrt.producer.import.name";
    public override string DescriptionLocalizationKey => "lrt.producer.import.description";
    public override Type InputType => typeof(ProducerImportInputState);
    public override Type StateType => typeof(ProducerImportState);

    protected override async Task DoWork()
    {
        var state = await GetStateAsync<ProducerImportState>()
                    ?? throw new InvalidOperationException("Producer import state is empty.");

        await using var stream = await s3Service.DownloadFileAsync(
            BucketNames.Uploads,
            state.FileName,
            CancellationToken);

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var rowIdx = 1;
        var errors = state.Errors;
        var producersToAdd = new List<Producer>();

        await foreach (var row in csv.GetRecordsAsync<NewProducerCsvDto>(CancellationToken))
        {
            if (rowIdx <= state.CurrentLine)
            {
                rowIdx++;
                continue;
            }
            var producer = ProcessRow(rowIdx, row, errors);

            if (errors.Count >= MaxErrors)
            {
                await UpdateState(state with
                {
                    CurrentLine = rowIdx,
                    Errors = errors
                });

                Interrupt(stringLocalizer.Get("producer.too.many.errors.while.processing.batch"));
            }

            if (producer != null)
                producersToAdd.Add(producer);

            if (producersToAdd.Count >= BatchSize)
            {
                await InsertAndClear(producersToAdd, CancellationToken);
                await UpdateState(state with
                {
                    CurrentLine = rowIdx,
                    Errors = errors
                });
            }

            rowIdx++;
        }

        if (producersToAdd.Count > 0)
            await InsertAndClear(producersToAdd, CancellationToken);

        await UpdateState(state with
        {
            CurrentLine = rowIdx - 1,
            Errors = errors
        });
    }


    private async Task InsertAndClear(
        List<Producer> producers,
        CancellationToken cancellationToken)
    {
        await producerRepository.BulkInsertOnConflictDoNothing(producers, cancellationToken);
        producers.Clear();
    }

    private Producer? ProcessRow(
        int rowIdx,
        NewProducerCsvDto row,
        List<ProducerImportError> errors)
    {
        try
        {
            return Producer.Create(row.Name, row.Description);
        }
        catch (Exception ex)
        {
            var message = ex is ILocalizableException localizableException
                ? stringLocalizer.GetOrDefault(localizableException.MessageKey) ?? ex.Message
                : ex.Message;

            errors.Add(new ProducerImportError
            {
                RowIdx = rowIdx,
                Message = message
            });

            return null;
        }
    }
    
    private record NewProducerCsvDto
    {
        [Name("Name")]
        public required string Name { get; init; }

        [Name("Description")]
        public string? Description { get; init; }
    }
}
