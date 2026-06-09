using System.Globalization;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;
using Main.Application.Dtos.Producer;
using Main.Application.Handlers.Producers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.ProducerImport;

public class ProducerImportLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    ISender sender,
    ILogger<ProducerImportLrt> logger,
    IScopedStringLocalizer stringLocalizer)
    : LrtNamedObjectBase(jobRepository, unitOfWork, logger)
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
        var rowsToAdd = new List<(int idx, NewProducerCsvDto row)>();

        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            if (rowIdx <= state.CurrentLine)
            {
                rowIdx++;
                continue;
            }

            if (errors.Count >= MaxErrors)
            {
                await UpdateState(state with
                {
                    CurrentLine = rowIdx,
                    Errors = errors
                });

                Interrupt(stringLocalizer.Get("producer.too.many.errors.while.processing.batch"));
            }

            NewProducerCsvDto row;
            try
            {
                row = csv.GetRecord<NewProducerCsvDto>();
            }
            catch (Exception ex) when (ex is CsvHelperException or TypeConverterException)
            {
                errors.Add(new ProducerImportError
                {
                    RowIdx = rowIdx,
                    Message = ex.Message
                });

                rowIdx++;
                continue;
            }
            
            rowsToAdd.Add((rowIdx, row));

            if (rowsToAdd.Count >= BatchSize)
            {
                await InsertAndClear(rowsToAdd, errors);
                await UpdateState(state with
                {
                    CurrentLine = rowIdx,
                    Errors = errors
                });
            }

            rowIdx++;
        }

        if (rowsToAdd.Count > 0)
            await InsertAndClear(rowsToAdd, errors);

        await UpdateState(state with
        {
            CurrentLine = rowIdx - 1,
            Errors = errors
        });
    }


    private async Task InsertAndClear(
        List<(int idx, NewProducerCsvDto row)> producers,
        List<ProducerImportError> errors)
    {
        if (producers.Count == 0) return;

        var firstIdx = producers[0].idx;
        
        var result = (await sender.Send(
            new CreateProducerBatchCommand(
                producers.Select(x => new NewProducerDto
                {
                    Name = x.row.Name,
                    Description = x.row.Description
                })),
            CancellationToken));

        foreach (var (idx, message) in result.Errors)
        {
            errors.Add(new ProducerImportError
            {
                Message = message,
                RowIdx = firstIdx + idx
            });
        }
        
        Logger.LogInformation(
            "Producer import batch processed. JobId: {JobId}, " +
            "BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
            "Created: {Created}, Skipped: {Skipped}, Errors: {Errors}",
            JobId,
            firstIdx,
            producers.Count,
            result.Created,
            result.Skipped,
            result.Errors.Count);
        
        producers.Clear();
    }
    
    private record NewProducerCsvDto
    {
        [Name("Name")]
        public required string Name { get; init; }

        [Name("Description")]
        public string? Description { get; init; }
    }
}
