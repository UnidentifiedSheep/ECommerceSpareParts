namespace Integrations.Tmtr.Requests;

public record GetProductsRequest
{
    public required string Number { get; init; }
}