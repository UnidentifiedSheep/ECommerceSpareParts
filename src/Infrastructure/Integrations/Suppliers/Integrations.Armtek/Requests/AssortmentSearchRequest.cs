namespace Integrations.Armtek.Requests;

public record AssortmentSearchRequest
{
	public required string Number { get; init; }
}
