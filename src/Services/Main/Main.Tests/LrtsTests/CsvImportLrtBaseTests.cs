using System.Text.Json;
using System.Text.Json.Serialization;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using SchemaGeneration.Abstractions.Attributes;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Localization.Abstractions.Interfaces;
using Main.Application.Lrts.Base;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tests.Abstractions.Test;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests.LrtsTests;

public sealed class CsvImportLrtBaseTests
    : LrtIntegrationTest<CsvImportLrtBaseTests.TestCsvImportLrt>
{
    private const string UploadsBucket = "uploads";

    public CsvImportLrtBaseTests(CombinedContainerFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task CheckpointInterval_FlushesValidRows_AndBaseClearsBatch()
    {
        var execution = await ExecuteCsv(
            false,
            "first",
            "invalid",
            "third",
            "fourth");

        execution.Job.Status.Should().Be(JobStatus.Succeeded);
        var state = execution.GetState<TestCsvImportState>();
        state.CurrentLine.Should().Be(4);
        state.ProcessedLines.Should().Equal(1, 3, 4);
        state.Errors.Should().ContainSingle(x => x.RowIdx == 2);
    }

    [Fact]
    public async Task ErrorLimit_CheckpointsProcessedRowBeforeFailing()
    {
        var execution = await ExecuteCsv(
            true,
            "invalid",
            "must-not-be-processed");

        execution.Job.Status.Should().Be(JobStatus.Failed);
        var state = execution.GetState<TestCsvImportState>();
        state.CurrentLine.Should().Be(1);
        state.ProcessedLines.Should().BeEmpty();
        state.Errors.Should().ContainSingle(x => x.RowIdx == 1);
    }

    private async Task<LrtExecutionResult> ExecuteCsv(
        bool stopOnFirstError,
        params string[] rows)
    {
        var fileName = $"{Guid.NewGuid():N}.csv";
        var csv = string.Join(
            Environment.NewLine,
            new[] { "Value" }.Concat(rows));
        Scope.ServiceProvider
            .GetRequiredService<S3StorageServiceStub>()
            .SetFile(UploadsBucket, fileName, csv);

        return await ExecuteLrt(
            JsonSerializer.Serialize(
                new TestCsvImportInputState
                {
                    FileName = fileName,
                    StopOnFirstError = stopOnFirstError
                }));
    }

    public sealed class TestCsvImportLrt(
        IRepository<Job, Guid> jobRepository,
        IUnitOfWork unitOfWork,
        IS3StorageService s3Service,
        ILogger<TestCsvImportLrt> logger,
        IOptions<S3BucketsOptions> bucketsOptions,
        IPublishEndpoint publisher,
        IApplicationTransactionService transactionService,
        IScopedStringLocalizer stringLocalizer)
        : CsvImportLrtBase<
            TestCsvImportInputState,
            TestCsvImportState,
            TestCsvRow,
            string>(
            jobRepository,
            bucketsOptions,
            unitOfWork,
            publisher,
            transactionService,
            logger,
            s3Service,
            stringLocalizer)
    {
        protected override int BatchSize => 100;
        protected override int CheckpointInterval => 2;
        protected override int MaxErrors => State.StopOnFirstError ? 1 : 100;
        public override string SystemName => nameof(TestCsvImportLrt);
        public override string NameLocalizationKey => "test.csv.import.name";
        public override string DescriptionLocalizationKey => "test.csv.import.description";

        protected override string GetTooManyErrorsLocalizationKey()
            => "producer.too.many.errors.while.processing.batch";

        protected override bool TryProcessRow(
            int rowIdx,
            TestCsvRow row,
            TestCsvImportState state,
            List<CsvImportError> errors,
            out string item)
        {
            item = row.Value;
            if (row.Value != "invalid") return true;

            errors.Add(CreateError(rowIdx, "Invalid row"));
            return false;
        }

        protected override Task ProcessBatch(
            IReadOnlyList<(int idx, string item)> items,
            TestCsvImportState state,
            List<CsvImportError> errors)
        {
            state.ProcessedLines.AddRange(items.Select(x => x.idx));
            return Task.CompletedTask;
        }
    }

    public sealed record TestCsvRow
    {
        [Name("Value")]
        public required string Value { get; init; }
    }

    public sealed record TestCsvImportState : TestCsvImportInputState,
        ICsvImportState<TestCsvImportState>
    {
        [JsonPropertyName("currentLine")]
        public int CurrentLine { get; init; }

        [JsonPropertyName("errors")]
        public List<CsvImportError> Errors { get; init; } = [];

        [JsonPropertyName("processedLines")]
        public List<int> ProcessedLines { get; init; } = [];

        public TestCsvImportState WithCurrentLine(int currentLine)
            => this with { CurrentLine = currentLine };
    }

    [CsvSchema(typeof(TestCsvRow))]
    public record TestCsvImportInputState : IInputState, ICsvImportInputState
    {
        [JsonPropertyName("fileName")]
        public required string FileName { get; init; }

        [JsonPropertyName("stopOnFirstError")]
        public bool StopOnFirstError { get; init; }

        public void ValidateState() { }
    }
}
