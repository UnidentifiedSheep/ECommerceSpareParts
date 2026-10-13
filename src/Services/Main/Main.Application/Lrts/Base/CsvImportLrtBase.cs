using System.Globalization;
using System.Text;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Application.Common.Models.Options.S3;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Domain.CommonEntities.Job;
using Exceptions.Interfaces;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.Base;

public abstract class CsvImportLrtBase<TInputState, TState, TCsvRow, TBatchItem>(
	IRepository<Job, Guid> jobRepository,
	IOptions<S3BucketsOptions> bucketsOptions,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	ILogger logger,
	IS3Service s3Service,
	IContextualLocalizer stringLocalizer) : LrtBase<TInputState, TState>(
	jobRepository,
	unitOfWork,
	publisher,
	transactionService,
	logger)
	where TInputState : class, IInputState, ICsvImportInputState
	where TState : class, ICsvImportState<TState>
{
	protected virtual int BatchSize => 1000;

	protected virtual int CheckpointInterval => BatchSize;

	protected virtual int MaxErrors => 10_000;
	protected abstract ILocalizableMessage GetTooManyErrorsLocalizationMessage { get; }

	protected IContextualLocalizer StringLocalizer => stringLocalizer;

	protected sealed override async Task DoWork(TInputState inputState)
	{
		var state = State;

		await BeforeRead(state);
		var checkpoint = state.Checkpoint;
		var offset = checkpoint?.Offset ?? 0;

		using var response = await s3Service.DownloadFileAsync(
			bucketsOptions.Value.Uploads.Name,
			inputState.FileName,
			offset,
			checkpoint?.ETag,
			CancellationToken);

		var streamResponse = response.Value ??
			throw new FileNotFoundException($"S3 object '{inputState.FileName}' was not found.");
		var eTag = streamResponse.ETag ??
			throw new InvalidOperationException("S3 response did not include an ETag for the CSV file.");
		if (checkpoint is not null && checkpoint.ETag != eTag)
			throw new InvalidOperationException("CSV source file changed after the last checkpoint.");

		var source = checkpoint is null
			? streamResponse.Stream
			: new PrefixStream(Encoding.UTF8.GetBytes(checkpoint.RawHeader), streamResponse.Stream);

		using var reader = new StreamReader(
			source,
			new UTF8Encoding(false, true),
			detectEncodingFromByteOrderMarks: false);
		using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			CountBytes = true,
			Encoding = Encoding.UTF8,
			PrepareHeaderForMatch = args => args.FieldIndex == 0
				? args.Header.TrimStart('\uFEFF')
				: args.Header
		});

		var rowIdx = checkpoint?.CurrentLine ?? 0;
		var errors = state.Errors;
		var rowsToAdd = new List<(int idx, TBatchItem item)>();

		if (!await csv.ReadAsync()) return;
		csv.ReadHeader();
		var rawHeader = csv.Parser.RawRecord;
		var initialByteCount = checkpoint is null ? 0 : csv.Parser.ByteCount;
		var surrogateCorrection = checkpoint is null ? CountSurrogatePairs(csv.Parser.RawRecord) * 2L : 0;
		var currentOffset = checkpoint?.Offset ?? csv.Parser.ByteCount - surrogateCorrection;

		while (await csv.ReadAsync())
		{
			rowIdx++;
			surrogateCorrection += CountSurrogatePairs(csv.Parser.RawRecord) * 2L;
			currentOffset = offset + csv.Parser.ByteCount - initialByteCount - surrogateCorrection;
			if (errors.Count >= MaxErrors)
				Interrupt(stringLocalizer.Get(GetTooManyErrorsLocalizationMessage));

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
			if (rowsToAdd.Count >= BatchSize || rowIdx - (state.Checkpoint?.CurrentLine ?? 0) >= CheckpointInterval ||
				errorLimitReached)
				state = await ProcessBatchAndSaveState(
					rowsToAdd,
					state,
					rowIdx,
					currentOffset,
					rawHeader,
					eTag,
					errors);

			if (errorLimitReached)
				Interrupt(stringLocalizer.Get(GetTooManyErrorsLocalizationMessage));
		}

		if (rowsToAdd.Count > 0 || (state.Checkpoint?.CurrentLine ?? 0) != rowIdx || state.Checkpoint is null)
			await ProcessBatchAndSaveState(
				rowsToAdd,
				state,
				rowIdx,
				currentOffset,
				rawHeader,
				eTag,
				errors);
	}

	protected virtual Task BeforeRead(TState state) => Task.CompletedTask;


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
			RowIdx = rowIdx, Message = message
		};
	}

	protected string GetErrorMessage(Exception ex) =>
		ex is ILocalizableException localizableException
			? stringLocalizer.Get(localizableException.LocalizableMessage)
			: ex.Message;

	private async Task<TState> ProcessBatchAndSaveState(
		List<(int idx, TBatchItem item)> items,
		TState state,
		int currentLine,
		long offset,
		string rawHeader,
		string eTag,
		List<CsvImportError> errors)
	{
		if (items.Count > 0)
			await ProcessBatch(
				items,
				state,
				errors);

		items.Clear();
		var updatedState = state.WithCheckpoint(new CsvImportCheckpoint
		{
			CurrentLine = currentLine,
			Offset = offset,
			RawHeader = rawHeader,
			ETag = eTag
		});
		await SaveStateAsync(updatedState);
		return updatedState;
	}

	private static int CountSurrogatePairs(string rawRecord)
	{
		var count = 0;
		for (var i = 0; i < rawRecord.Length - 1; i++)
			if (char.IsHighSurrogate(rawRecord[i]) && char.IsLowSurrogate(rawRecord[i + 1]))
			{
				count++;
				i++;
			}
		return count;
	}

	private sealed class PrefixStream(byte[] prefix, Stream source) : Stream
	{
		private int _position;
		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();
		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}
		public override int Read(byte[] buffer, int offset, int count) =>
			Read(buffer.AsSpan(offset, count));
		public override int Read(Span<byte> buffer)
		{
			if (_position >= prefix.Length) return source.Read(buffer);
			var count = Math.Min(buffer.Length, prefix.Length - _position);
			prefix.AsSpan(_position, count).CopyTo(buffer);
			_position += count;
			return count;
		}
		public override ValueTask<int> ReadAsync(
			Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			if (_position >= prefix.Length) return source.ReadAsync(buffer, cancellationToken);
			var count = Math.Min(buffer.Length, prefix.Length - _position);
			prefix.AsMemory(_position, count).CopyTo(buffer);
			_position += count;
			return ValueTask.FromResult(count);
		}
		public override void Flush() { }
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
		protected override void Dispose(bool disposing)
		{
			if (disposing) source.Dispose();
			base.Dispose(disposing);
		}
	}
}
