namespace Favorit.Integrations.Core.Requests;

public record GetPricesRequest
{
    public required string Number { get; init; }
    public string? Brand { get; init; }
    public bool ShowAnalogues { get; init; }
    public bool ShowIsRefundable { get; init; } 
}