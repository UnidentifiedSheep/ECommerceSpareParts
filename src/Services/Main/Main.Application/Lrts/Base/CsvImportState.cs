using System.Text.Json.Serialization;

namespace Main.Application.Lrts.Base;

public interface ICsvImportInputState
{
    string FileName { get; }
}

public interface ICsvImportState<TState> : ICsvImportInputState
{
    int CurrentLine { get; }
    List<CsvImportError> Errors { get; }

    TState WithCurrentLine(int currentLine);
}

public sealed record CsvImportError
{
    [JsonPropertyName("rowIdx")]
    public int RowIdx { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}
