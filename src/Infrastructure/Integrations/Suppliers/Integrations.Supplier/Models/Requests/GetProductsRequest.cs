namespace Integrations.Supplier.Models.Requests;

public record GetProductsRequest
{
    public required string Number { get; init; }
    public string? Brand { get; init; }
    
    public bool ShowAnalogues { get; init; }
}