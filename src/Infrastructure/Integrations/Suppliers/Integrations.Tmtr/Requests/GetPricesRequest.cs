namespace Integrations.Tmtr.Requests;

public record GetPricesRequest
{
	public required string Number { get; init; }

	public required string Brand { get; init; }
}
