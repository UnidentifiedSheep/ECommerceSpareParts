using System.Text.Json;
using Application.Common.LRT;

namespace Main.Application.Lrts.GenerateDocumen;

public record GenerateDocumentState : NoneInputState
{
	public required string DocumentSystemName { get; init; }

	public required JsonDocument DocumentRequest { get; init; }
}
