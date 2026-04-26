namespace Abstractions.Models;

public record Cursor<TCursor>(TCursor CursorValue, int Size);