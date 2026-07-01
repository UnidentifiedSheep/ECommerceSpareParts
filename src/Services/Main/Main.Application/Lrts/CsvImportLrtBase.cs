using System.Globalization;
using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using CsvHelper;
using CsvHelper.TypeConversion;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;
using Localization.Domain;
using Main.Application.Static;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts;

public abstract class CsvImportLrtBase<TState, TError, TCsvRow, TBatchItem>(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    ILogger logger,
    IS3StorageService s3Service,
    IScopedStringLocalizer stringLocalizer,
    IOptions<LocalesOptions> localesOptions
)
    : LrtNamedObjectBase(
        jobRepository,
        unitOfWork,
        publisher,
        logger)
{
    protected virtual int BatchSize => 1000;
    protected virtual int MaxErrors => 10_000;
    protected IScopedStringLocalizer StringLocalizer => stringLocalizer;

    protected override IServiceDefinition ServiceDefinition => ServicesDefinitions.Main;

    protected sealed override async Task DoWork()
    {
        stringLocalizer.SetLocale(localesOptions.Value.Default);
        var state = await GetStateAsync<TState>()
                    ?? throw new InvalidOperationException($"{GetType().Name} state is empty.");

        await BeforeRead(state);

        await using var stream = await s3Service.DownloadFileAsync(
            BucketNames.Uploads,
            GetFileName(state),
            CancellationToken);

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var rowIdx = 1;
        var errors = GetErrors(state);
        var rowsToAdd = new List<(int idx, TBatchItem item)>();

        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            if (rowIdx <= GetCurrentLine(state))
            {
                rowIdx++;
                continue;
            }

            if (errors.Count >= MaxErrors)
            {
                await UpdateState(
                    WithUpdatedState(
                        state,
                        rowIdx,
                        errors));
                Interrupt(stringLocalizer.Get(GetTooManyErrorsLocalizationKey()));
            }

            TCsvRow row;
            try { row = csv.GetRecord<TCsvRow>(); }
            catch (Exception ex) when (ex is CsvHelperException or TypeConverterException)
            {
                errors.Add(CreateError(rowIdx, ex.Message));
                rowIdx++;
                continue;
            }

            if (TryProcessRow(
                    rowIdx,
                    row,
                    state,
                    errors,
                    out var item))
                rowsToAdd.Add((rowIdx, item));

            if (rowsToAdd.Count >= BatchSize)
            {
                await ProcessBatch(
                    rowsToAdd,
                    state,
                    errors);
                await UpdateState(
                    WithUpdatedState(
                        state,
                        rowIdx,
                        errors));
            }

            rowIdx++;
        }

        if (rowsToAdd.Count > 0)
            await ProcessBatch(
                rowsToAdd,
                state,
                errors);

        await UpdateState(
            WithUpdatedState(
                state,
                rowIdx - 1,
                errors));
    }

    protected virtual Task BeforeRead(TState state) { return Task.CompletedTask; }

    protected abstract string GetFileName(TState state);
    protected abstract int GetCurrentLine(TState state);
    protected abstract List<TError> GetErrors(TState state);
    protected abstract TError CreateError(int rowIdx, string message);
    protected abstract string GetTooManyErrorsLocalizationKey();

    protected abstract TState WithUpdatedState(
        TState state,
        int currentLine,
        List<TError> errors);

    protected abstract bool TryProcessRow(
        int rowIdx,
        TCsvRow row,
        TState state,
        List<TError> errors,
        out TBatchItem item);

    protected abstract Task ProcessBatch(
        List<(int idx, TBatchItem item)> items,
        TState state,
        List<TError> errors);
}