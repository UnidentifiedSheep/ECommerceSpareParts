namespace Integrations.Supplier.Connections;

public record ArmtekConnection
{
	public required string BaseUrl { get; init; }

	public required string Login { get; init; }

	public required string Password { get; init; }

	public required string SalesOrganization { get; init; }

	public required string CustomerCode { get; init; }

	public string? ContractCode { get; init; }
}
