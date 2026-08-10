using System.Globalization;
using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Application.Common.Models.Options.S3;
using Application.Common.NamedObject;
using CsvHelper;
using CsvHelper.TypeConversion;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using Localization.Abstractions.Interfaces;
using Main.Application.Static;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts.Base;

public abstract class CsvImportLrtBase<TInputState, TState, TCsvRow, TBatchItem>(
    IRepository<Job, Guid> jobRepository,
    IOptions<S3BucketsOptions> bucketsOptions,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    IApplicationTransactionService transactionService,
    ILogger logger,
    IS3StorageService s3Service,
    IScopedStringLocalizer stringLocalizer
) : LrtBase<TInputState, TState>(
        jobRepository,
        unitOfWork,
        publisher,
        transactionService,
        logger)
    where TInputState : class, IInputState, ICsvImportInputState
    where TState : class, TInputState, ICsvImportState<TState>
{
    protected virtual int BatchSize => 1000;
    protected virtual int CheckpointInterval => BatchSize;
    protected virtual int MaxErrors => 10_000;
    protected IScopedStringLocalizer StringLocalizer => stringLocalizer;

    protected sealed override async Task DoWork()
    {
        var state = State;

        await BeforeRead(state);

        await using var stream = await s3Service.DownloadFileAsync(
            bucketsOptions.Value.Uploads.Name,
            state.FileName,
            CancellationToken);

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var rowIdx = 0;
        var errors = state.Errors;
        var rowsToAdd = new List<(int idx, TBatchItem item)>();

        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            rowIdx++;
            if (rowIdx <= state.CurrentLine) continue;

            if (errors.Count >= MaxErrors)
                Interrupt(stringLocalizer.Get(GetTooManyErrorsLocalizationKey()));

            var rowParsed = true;
            TCsvRow row = default!;
            try
            {
                row = csv.GetRecord<TCsvRow>();
            }
            catch (Exception ex) when (ex is CsvHelperException or TypeConverterException)
            {
                errors.Add(CreateError(rowIdx, ex.Message));
                rowParsed = false;
            }

            if (rowParsed && TryProcessRow(
                    rowIdx,
                    row,
                    state,
                    errors,
                    out var item))
                rowsToAdd.Add((rowIdx, item));

            var errorLimitReached = errors.Count >= MaxErrors;
            if (rowsToAdd.Count >= BatchSize ||
                rowIdx - state.CurrentLine >= CheckpointInterval ||
                errorLimitReached)
            {
                state = await ProcessBatchAndSaveState(
                    rowsToAdd,
                    state,
                    rowIdx,
                    errors);
            }

            if (errorLimitReached)
                Interrupt(stringLocalizer.Get(GetTooManyErrorsLocalizationKey()));
        }

        if (rowsToAdd.Count > 0 || state.CurrentLine != rowIdx)
            await ProcessBatchAndSaveState(
                rowsToAdd,
                state,
                rowIdx,
                errors);
    }

    protected virtual Task BeforeRead(TState state) { return Task.CompletedTask; }

    protected abstract string GetTooManyErrorsLocalizationKey();

    protected abstract bool TryProcessRow(
        int rowIdx,
        TCsvRow row,
        TState state,
        List<CsvImportError> errors,
        out TBatchItem item);

    protected abstract Task ProcessBatch(
        IReadOnlyList<(int idx, TBatchItem item)> items,
        TState state,
        List<CsvImportError> errors);

    protected static CsvImportError CreateError(int rowIdx, string message)
    {
        return new CsvImportError
        {
            RowIdx = rowIdx,
            Message = message
        };
    }

    private async Task<TState> ProcessBatchAndSaveState(
        List<(int idx, TBatchItem item)> items,
        TState state,
        int currentLine,
        List<CsvImportError> errors)
    {
        if (items.Count > 0)
            await ProcessBatch(items, state, errors);

        items.Clear();
        var updatedState = state.WithCurrentLine(currentLine);
        await SaveStateAsync(updatedState);
        return updatedState;
    }
}
