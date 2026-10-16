using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Abstractions.Interfaces;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Locan.Core.LocalizableMessages;
using Main.Application.Lrts.Base;
using Main.Entities;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;
using SchemaGeneration.Abstractions.Attributes;
using Tests.Abstractions.Test;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests.LrtsTests;

public sealed class CsvImportLrtBaseTests : LrtIntegrationTest<CsvImportLrtBaseTests.TestCsvImportLrt>
{
	private const string UploadsBucket = "uploads";

	public CsvImportLrtBaseTests(CombinedContainerFixture fixture) : base(fixture)
	{
	}

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
		state.Checkpoint!.CurrentLine.Should().Be(4);
		state
		.ProcessedLines
		.Should()
		.Equal(
			1,
			3,
			4);
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
		state.Checkpoint!.CurrentLine.Should().Be(1);
		state.ProcessedLines.Should().BeEmpty();
		state.Errors.Should().ContainSingle(x => x.RowIdx == 1);
	}

	[Fact]
	public async Task Retry_ResumesAtByteOffset_AfterBomUnicodeAndMultilineRecord()
	{
		var fileName = $"{Guid.NewGuid():N}.csv";
		var content = "\uFEFFValue\n🙂\n\"two\nlines\"\nretry-once\nlast";
		var storage = Scope.ServiceProvider.GetRequiredService<S3StorageServiceStub>();
		storage.SetFile(UploadsBucket, fileName, content);

		var execution = await ExecuteLrt(
			JsonSerializer.Serialize(new TestCsvImportInputState { FileName = fileName }),
			cancellationToken: CancellationToken);

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<TestCsvImportState>();
		state.ProcessedLines.Should().Equal(1, 2, 3, 4);
		state.Checkpoint!.CurrentLine.Should().Be(4);
		state.Checkpoint!.Offset.Should().Be(Encoding.UTF8.GetByteCount(content));
		state.Checkpoint.RawHeader.Should().Be("\uFEFFValue\n");
		storage.GetDownloadOffsets(UploadsBucket, fileName).Should().Equal(
			0,
			Encoding.UTF8.GetByteCount("\uFEFFValue\n🙂\n\"two\nlines\"\n"));
	}

	[Fact]
	public async Task Retry_AfterFinalCheckpoint_ResumesFromEndOfFile()
	{
		var fileName = $"{Guid.NewGuid():N}.csv";
		var content = "Value\nfail-on-succeed\nsecond";
		var storage = Scope.ServiceProvider.GetRequiredService<S3StorageServiceStub>();
		storage.SetFile(UploadsBucket, fileName, content);

		var execution = await ExecuteLrt(
			JsonSerializer.Serialize(new TestCsvImportInputState { FileName = fileName }),
			cancellationToken: CancellationToken);

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<TestCsvImportState>();
		state.ProcessedLines.Should().Equal(1, 2);
		state.Checkpoint!.Offset.Should().Be(Encoding.UTF8.GetByteCount(content));
		storage.GetDownloadOffsets(UploadsBucket, fileName).Should().Equal(0, state.Checkpoint.Offset);
	}

	[Fact]
	public async Task Retry_PreservesQuotedHeader()
	{
		var fileName = $"{Guid.NewGuid():N}.csv";
		var content = "\"Val,ue\"\nfirst\nsecond\nretry-once\nlast";
		var storage = Scope.ServiceProvider.GetRequiredService<S3StorageServiceStub>();
		storage.SetFile(UploadsBucket, fileName, content);

		var execution = await ExecuteLrt(
			JsonSerializer.Serialize(new TestCsvImportInputState { FileName = fileName }),
			cancellationToken: CancellationToken);

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<TestCsvImportState>();
		state.ProcessedLines.Should().Equal(1, 2, 3, 4);
		state.Checkpoint!.RawHeader.Should().Be("\"Val,ue\"\n");
		storage.GetDownloadOffsets(UploadsBucket, fileName).Should().Equal(
			0,
			Encoding.UTF8.GetByteCount("\"Val,ue\"\nfirst\nsecond\n"));
	}

	private async Task<LrtExecutionResult> ExecuteCsv(bool stopOnFirstError, params string[] rows)
	{
		var fileName = $"{Guid.NewGuid():N}.csv";
		var csv = string.Join(
			Environment.NewLine,
			new[]
			{
				"Value"
			}.Concat(rows));
		Scope
		.ServiceProvider
		.GetRequiredService<S3StorageServiceStub>()
		.SetFile(
			UploadsBucket,
			fileName,
			csv);

		return await ExecuteLrt(
			JsonSerializer.Serialize(
				new TestCsvImportInputState
				{
					FileName = fileName, StopOnFirstError = stopOnFirstError
				}));
	}

	public sealed class TestCsvImportLrt(
		IRepository<Job, Guid> jobRepository,
		IUnitOfWork unitOfWork,
		IS3Service s3Service,
		ILogger<TestCsvImportLrt> logger,
		IOptions<S3BucketsOptions> bucketsOptions,
		IPublishEndpoint publisher,
		IApplicationTransactionService transactionService,
		IContextualLocalizer stringLocalizer)
		: CsvImportLrtBase<TestCsvImportInputState, TestCsvImportState, TestCsvDto, string>(
			jobRepository,
			bucketsOptions,
			unitOfWork,
			publisher,
			transactionService,
			logger,
			s3Service,
			stringLocalizer)
	{
		private bool _failedOnce;
		private bool _failOnSucceed;
		private bool _failedOnSucceed;

		protected override int BatchSize => 100;

		protected override int CheckpointInterval => 2;

		protected override int MaxErrors => State.StopOnFirstError ? 1 : 100;

		public override string SystemName => nameof(TestCsvImportLrt);

		public override ILocalizableMessage NameLocalizationMessage =>
			new LocalizableMessage("test.csv.import.name");

		public override ILocalizableMessage DescriptionLocalizationMessage =>
			new LocalizableMessage("test.csv.import.description");

		protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
			ProducerTooManyErrorsWhileProcessingBatchMessage.Instance;

		protected override bool TryProcessRow(
			int rowIdx,
			TestCsvDto row,
			TestCsvImportState state,
			List<CsvImportError> errors,
			out string item)
		{
			if (row.Value == "fail-on-succeed") _failOnSucceed = true;
			if (row.Value == "retry-once" && !_failedOnce)
			{
				_failedOnce = true;
				throw new InvalidOperationException("Transient failure after checkpoint.");
			}

			item = row.Value;
			if (row.Value != "invalid")
				return true;

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

		protected override Task SucceedJobAsync()
		{
			if (_failOnSucceed && !_failedOnSucceed)
			{
				_failedOnSucceed = true;
				throw new InvalidOperationException("Transient failure after final checkpoint.");
			}
			return base.SucceedJobAsync();
		}
	}

	public sealed record TestCsvDto
	{
		[Name("Value", "Val,ue")]
		public required string Value { get; init; }
	}

	public sealed record TestCsvImportState : TestCsvImportInputState, ICsvImportState<TestCsvImportState>
	{

		[JsonPropertyName("processedLines")]
		public List<int> ProcessedLines { get; init; } = [];

		[JsonPropertyName("errors")]
		public List<CsvImportError> Errors { get; init; } = [];

		[JsonPropertyName("checkpoint")]
		public CsvImportCheckpoint? Checkpoint { get; init; }

		public TestCsvImportState WithCheckpoint(CsvImportCheckpoint checkpoint) => this with
		{
			Checkpoint = checkpoint
		};
	}

	[CsvSchema(typeof(TestCsvDto))]
	public record TestCsvImportInputState : IInputState, ICsvImportInputState
	{

		[JsonPropertyName("stopOnFirstError")]
		public bool StopOnFirstError { get; init; }

		[JsonPropertyName("fileName")]
		public required string FileName { get; init; }

		public void ValidateState()
		{
		}
	}
}
