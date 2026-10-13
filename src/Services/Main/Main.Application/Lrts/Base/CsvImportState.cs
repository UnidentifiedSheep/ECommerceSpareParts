using System.Text.Json.Serialization;

namespace Main.Application.Lrts.Base;

public interface ICsvImportInputState
{
	string FileName { get; }
}

public interface ICsvImportState<TState> : ICsvImportInputState
{
	CsvImportCheckpoint? Checkpoint { get; }

	List<CsvImportError> Errors { get; }

	TState WithCheckpoint(CsvImportCheckpoint checkpoint);
}

public sealed record CsvImportCheckpoint
{
	[JsonPropertyName("offset")]
	public required long Offset { get; init; }

	[JsonPropertyName("rawHeader")]
	public required string RawHeader { get; init; }

	[JsonPropertyName("eTag")]
	public required string ETag { get; init; }

	[JsonPropertyName("currentLine")]
	public required int CurrentLine { get; init; }
}

public sealed record CsvImportError
{
	[JsonPropertyName("rowIdx")]
	public int RowIdx { get; init; }

	[JsonPropertyName("message")]
	public required string Message { get; init; }
}
